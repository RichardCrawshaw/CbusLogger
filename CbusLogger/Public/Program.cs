using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace Crowswood.CbusLogger
{
    class Program
    {
        #region Members

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly AutoResetEvent autoResetEvent = new(false);

        private static readonly ISettings settings = Resolver.Instance.Resolve<ISettings>();

        #endregion

        #region Methods

        static int Main(string[] args)
        {
            if (!GetArgs(args))
            {
                Console.WriteLine("Press [Enter] to exit.");
                Console.ReadLine();
                return 0;
            }

            if (!Validate(out var returnCode))
                return returnCode;

            DisplaySettings();

            // Start the receive processor running.
            ThreadPool.QueueUserWorkItem(x => Process());

            // Wait for the user to close the application.
            Console.WriteLine("Logging incoming messages.");
            Console.WriteLine("Press [Enter] to exit.");
            Console.ReadLine();

            // Trigger the receive routine to complete and thus disconnect the processor.
            autoResetEvent.Set();

            // Wait for the receive routine to disconnect.
            autoResetEvent.WaitOne();

            return 0;
        }

        #endregion

        #region Support routines

        private static void DisplayComPorts()
        {
            var ports = SerialPort.GetPortNames();
            Console.WriteLine($"{ports.Length} COM ports found:");
            foreach (var port in ports)
                Console.WriteLine(port);
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("/cn\tSets COM Port n, where n > 0");
            Console.WriteLine("/d\tDisplays all the available ports");
            Console.WriteLine("/l[sgc][+-]\tSets the logging:");
            Console.WriteLine("  \ts = Serial port | g = GridConnect | c = CBUS");
            Console.WriteLine("  \t+ = enable (default) | - = disable");
            Console.WriteLine("/on\tControls which op-codes are logged; where n is the op-code.");
            Console.WriteLine("  \tIf any op-codes are specified all others are excluded.");
            Console.WriteLine("/?\tDisplays this help message.");
        }

        private static void DisplaySettings()
        {
            Console.WriteLine($"Using COM{settings.PortNumber}");

            var targetFilenames = GetTargetFilenames();
            if (!targetFilenames.Any())
                Console.WriteLine("Not logging to any file; check the NLog configuration in NLog.config.");
            else
                foreach (var filename in targetFilenames)
                    Console.WriteLine($"Logging to {filename}.");

            if (settings.LoggingSerial) Console.WriteLine("Logging serial data.");
            if (settings.LoggingGridConnect) Console.WriteLine("Logging GridConnect data.");
            if (settings.LoggingCbus) Console.WriteLine("Logging CBUS data.");
        }

        private static bool GetArgs(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg[0] != '/') continue;
                switch (arg[1])
                {
                    case 'c': // Specify COM port
                    case 'C':
                        if (int.TryParse(arg[2..], out var comPortNumber))
                            settings.PortNumber = comPortNumber;
                        break;
                    case 'd': // Display available COM ports
                    case 'D':
                        DisplayComPorts();
                        return false;
                    case 'l': // Control logging
                    case 'L':
                        if (arg.Length > 2)
                            GetLoggingSettings(arg[2..]);
                        break;
                    case 'o': // Op-Code logging
                    case 'O':
                        if (arg.Length > 2)
                            GetOpCodeSettings(arg[2..]);
                        break;
                    case '?': // Display help
                        DisplayHelp();
                        return false;
                }
            }

            return true;
        }

        private static bool GetLoggingSettings(string arg)
        {
            switch (arg[0])
            {
                case 's':
                case 'S':
                    if (arg[1] == '+')
                        settings.LoggingSerial = true;
                    else if (arg[1] == '-')
                        settings.LoggingSerial = false;
                    return true;
                case 'g':
                case 'G':
                    if (arg[1] == '+')
                        settings.LoggingGridConnect = true;
                    else if (arg[1] == '-')
                        settings.LoggingGridConnect = false;
                    return true;
                case 'c':
                case 'C':
                    if (arg[1] == '+')
                        settings.LoggingCbus = true;
                    else if (arg[1] == '-')
                        settings.LoggingCbus = false;
                    return true;
            }

            return false;
        }

        private static bool GetOpCodeSettings(string arg)
        {
            settings.OpCodes.Add(arg);

            return true;
        }

        private static List<string> GetTargetFilenames()
        {
            var results = new List<string>();
            foreach (var target in LogManager.Configuration.ConfiguredNamedTargets)
            {
                var namedTarget = target;
                while (target is WrapperTargetBase wrapperTarget)
                    namedTarget = wrapperTarget.WrappedTarget;
                if (namedTarget is FileTarget fileTarget)
                {
                    var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
                    var filename = fileTarget.FileName.Render(logEventInfo).Replace(@"\/", @"\").Replace("/", @"\");
                    results.Add(filename);
                }
            }
            return results;
        }

        private static void Process()
        {
            logger.Trace(() => nameof(Process));

            using var processor = Resolver.Instance.Resolve<ICbusProcessor>();

            try
            {
                processor.Connect();
                if (!processor.IsConnected)
                {
                    Console.WriteLine("Failed to connect CBUS processor.");
                    return;
                }

                // Wait for the program to be terminated.
                autoResetEvent.WaitOne();

                // Disconnect the processor.
                processor.Disconnect();
            }
            finally
            {
                // Trigger the main method that the processor has been disconnected.
                autoResetEvent.Set();

                logger.Trace(() => $"{nameof(Process)} terminated.");
            }
        }

        private static bool Validate(out int returnCode)
        {
            if (settings.PortNumber < 1)
            {
                logger.Fatal(() => $"Invalid COM Port number: {settings.PortNumber}.");
                returnCode = 1;
                return false;
            }

            var ports = SerialPort.GetPortNames();
            if (!ports.Any(p => $"COM{settings.PortNumber}" == p))
            {
                logger.Fatal(() => $"COM{settings.PortNumber} is not currently available.");
                returnCode = 2;
                return false;
            }

            returnCode = 0;
            return true;
        }

        #endregion
    }
}

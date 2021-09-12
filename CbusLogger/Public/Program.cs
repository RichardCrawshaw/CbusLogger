using System;
using System.Collections.Concurrent;
using System.IO;
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
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly AutoResetEvent autoResetEvent = new(false);

        private static int comPortNumber;
        //private static bool interpretMessages = false;

        static int Main(string[] args)
        {
            // Args
            // COM Port /c<int> eg /c1 to use COM1
            // List ports /d
            if (!GetArgs(args))
            {
                Console.WriteLine("Press [Enter] to exit.");
                Console.ReadLine();
                return 0;
            }

            if (comPortNumber < 1)
            {
                logger.Fatal(() => $"Invalid COM Port number: {comPortNumber}.");
                return 1;
            }

            Console.WriteLine($"Using COM{comPortNumber}");

            var target = LogManager.Configuration.FindTargetByName("file");
            while ((target != null) && (target is WrapperTargetBase))
                target = (target as WrapperTargetBase).WrappedTarget;
            if (target is FileTarget fileTarget)
            {
                var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now, };
                var filename = fileTarget.FileName.Render(logEventInfo);
                filename = filename.Replace(@"\/", @"\");
                filename = filename.Replace("/", @"\");
                Console.WriteLine($"Logging to {filename}");
            }

            ThreadPool.QueueUserWorkItem(LogIncomingMessages);
            Console.WriteLine("Logging incoming messages.");
            Console.WriteLine("Press [Enter] to exit.");

            Console.ReadLine();

            autoResetEvent.Set();

            return 0;
        }

        private static bool GetArgs(string[] args)
        {
            comPortNumber = 1;

            foreach(var arg in args)
            {
                if (arg[0] != '/') continue;
                switch (arg[1])
                {
                    case 'c':
                        _ = int.TryParse(arg[2..], out comPortNumber);
                        break;
                    case 'd':
                        var ports = SerialPort.GetPortNames();
                        Console.WriteLine($"{ports.Length} COM ports found:");
                        foreach (var port in ports)
                            Console.WriteLine(port);
                        return false;
                    //case 'i':
                    //    interpretMessages = true;
                    //    break;
                    case '?':
                        Console.WriteLine("/cn\tSets COM Port n, where n > 0");
                        Console.WriteLine("/d\tDisplays all the available ports");
                        //Console.WriteLine("/i\tInterprets messages");
                        Console.WriteLine("/?\tDisplays this help message.");
                        return false;
                }
            }

            return true;
        }

        private static void LogIncomingMessages(object state)
        {
            var portName = $"COM{comPortNumber}";
            var baudRate = 115200;
            var parity = Parity.None;
            var dataBits = 8;
            var stopBits = StopBits.One;
            var serial = new SerialPortAdapter(portName, baudRate, parity, dataBits, stopBits);
            serial.ReceivedSerialData += Serial_ReceivedSerialData;

            serial.Open();

            autoResetEvent.WaitOne();

            serial.Close();

            serial.ReceivedSerialData -= Serial_ReceivedSerialData;
        }

        private static void Serial_ReceivedSerialData(object sender, ReceivedSerialDataEventArgs e)
        {
            var message = new string(e.ReceivedSerialData.Select(d => (char)d).ToArray());
            logger.Info(() => message);
        }
    }
}

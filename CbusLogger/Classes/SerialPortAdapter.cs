using System;
using System.IO;
using System.IO.Ports;
using NLog;

namespace Crowswood.CbusLogger
{
    /// <summary>
    /// Adapter class for <see cref="System.IO.Ports.SerialPort"/> that fixes
    /// a number of its issues. See this blog post by Ben Voigt:
    /// https://www.sparxeng.com/blog/software/must-use-net-system-io-ports-serialport
    /// </summary>
    internal class SerialPortAdapter
    {
        #region Variables

        const int READ_BUFFER_SIZE = 64;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly SerialPort serialPort;

        #endregion

        #region Properties

        public int BaudRate => this.serialPort.BaudRate;

        public uint BufferSize { get; set; } = READ_BUFFER_SIZE;

        public int DataBits => this.serialPort.DataBits;

        public bool IsOpen => this.serialPort.IsOpen;

        public Parity Parity => this.serialPort.Parity;

        public string PortName => this.serialPort.PortName;

        public StopBits StopBits => this.serialPort.StopBits;

        public Stream Stream => this.serialPort?.BaseStream;

        #endregion

        #region Constructors

        public SerialPortAdapter(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            logger.Trace(() => $"Creating {portName} ({baudRate} {parity} {dataBits} {stopBits}).");

            this.serialPort = GetSerialPort(portName, baudRate, parity, dataBits, stopBits);
            if (this.serialPort == null)
                throw new IOException($"Failed to create port: {portName} ({baudRate} {parity} {dataBits} {stopBits}).");
        }

        #endregion

        #region Methods

        public static Parity ConvertParity(string parity) => Enum.TryParse<Parity>(parity, out var result) ? result : default;

        public static StopBits ConvertStopBits(string stopBits) => Enum.TryParse<StopBits>(stopBits, out var result) ? result : default;

        public void Close() => this.serialPort.Close();

        public void Open()
        {
            logger.Trace(() => $"Opening serial port: {this.serialPort.PortName}.");

            this.serialPort.Open();
            if (!this.serialPort.IsOpen) return;

            ReadIncomingData(this.serialPort);
        }

        public void Send(byte[] buffer) => this.serialPort.Write(buffer, 0, buffer.Length);

        public void Write(string text) => this.serialPort.Write(text);

        #endregion

        #region Events

        public event EventHandler<ReceivedSerialDataEventArgs> ReceivedSerialData;

        public event EventHandler<SerialPortErrorEventArgs> SerialPortError;

        #endregion

        #region Support routines

        private static SerialPort GetSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            var serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);

            return serialPort;
        }

        private void ReadIncomingData(SerialPort serialPort)
        {
            byte[] buffer = new byte[this.BufferSize];
            void kickoffRead(Stream stream)
            {
                stream.BeginRead(buffer, 0, buffer.Length,
                    delegate (IAsyncResult result)
                    {
                        try
                        {
                            if (serialPort.IsOpen)
                            {
                                var actualLength = stream.EndRead(result);
                                var received = new byte[actualLength];
                                Buffer.BlockCopy(buffer, 0, received, 0, actualLength);
                                this.ReceivedSerialData?.Invoke(serialPort,
                                    new ReceivedSerialDataEventArgs
                                    {
                                        ReceivedSerialData = received,
                                    });
                            }
                        }
                        catch (IOException ex)
                        {
                            if (serialPort.IsOpen)
                                this.SerialPortError?.Invoke(this,
                                    new SerialPortErrorEventArgs
                                    {
                                        Exception = ex,
                                    });
                        }
                        if (serialPort.IsOpen)
                            kickoffRead(stream);
                    },
                    null);
            }

            kickoffRead(serialPort.BaseStream);

            logger.Trace(() => $"{this.serialPort.PortName} waiting for incoming data.");
        }

        #endregion
    }

    public class ReceivedSerialDataEventArgs : EventArgs
    {
        public byte[] ReceivedSerialData { get; set; }
    }

    public class SerialPortErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
    }
}

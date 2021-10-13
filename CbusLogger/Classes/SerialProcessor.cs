using System;
using System.IO.Ports;
using System.Linq;
using NLog;

namespace Crowswood.CbusLogger
{
    /// <summary>
    /// Class to process serial messages.
    /// </summary>
    class SerialProcessor :
        ISerialProcessor
    {
        #region Members

        private readonly static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly SerialPortAdapter serialPortAdapter = null;

        #endregion

        #region Properties

        public bool IsDisposed { get; private set; }

        public bool IsOpen => this.serialPortAdapter?.IsOpen ?? false;

        #endregion

        #region Constructors

        public SerialProcessor()
        {
            logger.Trace(() => nameof(SerialProcessor));

            this.serialPortAdapter = new();
            this.serialPortAdapter.ReceivedSerialData += SerialPort_ReceivedSerialData;
        }

        #endregion

        #region IDisposable support

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)

                    if (this.serialPortAdapter != null)
                    {
                        this.serialPortAdapter.Close();
                        this.serialPortAdapter.ReceivedSerialData -= SerialPort_ReceivedSerialData;
                        this.serialPortAdapter.Dispose();
                    }
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                IsDisposed = true;
            }
        }

        // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SerialProcessor()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        public void Close()
        {
            logger.Trace(() => nameof(Close));

            if (this.serialPortAdapter.IsOpen)
                this.serialPortAdapter.Close();
        }

        public bool Open(string comPort)
        {
            logger.Trace(() => nameof(Open));
            logger.Debug(() => comPort);

            var baudRate = 115200;
            var dataBits = 8;
            var stopBits = StopBits.One;
            var parity = Parity.None;

            this.serialPortAdapter.Open(comPort, baudRate, dataBits, stopBits, parity);
            return serialPortAdapter.IsOpen;
        }

        #endregion

        #region Events

        public event EventHandler<SerialProcessorEventArgs> MessageReceived;

        #endregion

        #region Event handler routines

        private void SerialPort_ReceivedSerialData(object sender, ReceivedSerialDataEventArgs e)
        {
            this.MessageReceived?.Invoke(this,
                new SerialProcessorEventArgs
                {
                    Message = new string(e.ReceivedSerialData.Select(b => (char)b).ToArray()),
                });
        }

        #endregion
    }
}

using System;
using System.Linq;
using NLog;

namespace Crowswood.CbusLogger
{
    class LoggingSerialPortAdapter :
        SerialPortAdapter
    {
        #region Members

        /// <summary>
        /// Logger for standard program logging.
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Logger for recording incoming messages.
        /// </summary>
        private static readonly Logger msglog = LogManager.GetLogger("Message.Serial");

        private readonly ISettings settings;

        #endregion

        #region Constructors

        public LoggingSerialPortAdapter(ISettings settings) 
            : base()
        {
            logger.Trace(() => nameof(SerialPortAdapter));

            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            this.settings = settings;
        }

        #endregion

        #region Support routines

        /// <summary>
        /// Handles the logging and onward transmission of the specified <paramref name="received"/> data.
        /// </summary>
        /// <param name="received">The data that has been received.</param>
        protected override void OnReceivedSerialData(byte[] received)
        {
            if (this.settings.LoggingSerial)
                msglog.Info(() => string.Join(" ", received.Select(b => $"0x{b:X2}")));

            base.OnReceivedSerialData(received);
        }

        #endregion
    }
}

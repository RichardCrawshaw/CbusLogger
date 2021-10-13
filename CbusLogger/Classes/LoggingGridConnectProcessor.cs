using System;
using NLog;

namespace Crowswood.CbusLogger
{
    class LoggingGridConnectProcessor :
        GridConnectProcessor
    {
        #region Members

        /// <summary>
        /// Logger for recording incoming messages.
        /// </summary>
        private static readonly Logger msglog = LogManager.GetLogger("Message.GridConnect");

        /// <summary>
        /// The settings used to control the current instance.
        /// </summary>
        private readonly ISettings settings;

        #endregion

        #region Constructors

        public LoggingGridConnectProcessor(ISettings settings, ISerialPortAdapter serialPortAdapter)
            : base(serialPortAdapter)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            this.settings = settings;
        }

        #endregion

        #region Support routines

        /// <summary>
        /// Raise an event for the specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string containing the message.</param>
        protected override void OnMessageReceived(string message)
        {
            if (this.settings.LoggingGridConnect)
                msglog.Info(() => message);

            base.OnMessageReceived(message);
        }

        #endregion
    }
}

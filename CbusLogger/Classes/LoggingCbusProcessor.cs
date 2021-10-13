using System;
using System.Linq;
using NLog;

namespace Crowswood.CbusLogger
{
    internal class LoggingCbusProcessor : CbusProcessor
    {
        #region Fields

        /// <summary>
        /// Logger for recording incoming messages.
        /// </summary>
        private static readonly Logger msglog = LogManager.GetLogger("Message.CBUS");

        // The settings.
        private readonly ISettings settings;

        #endregion

        #region Constructors

        public LoggingCbusProcessor(ISettings settings, IGridConnectProcessor gridConnectProcessor)
            : base(gridConnectProcessor)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            this.settings = settings;

            base.PortNumber = settings.PortNumber;
        }

        #endregion

        #region Event handler routines

        protected override void GridConnectProcessor_MessageReceived(object sender, GridConnectMessageEventArgs e)
        {
            if (this.settings.LoggingCbus)
            {
                var message = GetMessage(e.Payload);
                if (!settings.OpCodes.Any() || settings.OpCodes.Any(n => n.Equals(message.OpCode, StringComparison.InvariantCultureIgnoreCase)))
                    msglog.Info(() => InterpretMessage(message));
            }

            base.GridConnectProcessor_MessageReceived(sender, e);
        }

        #endregion
    }
}

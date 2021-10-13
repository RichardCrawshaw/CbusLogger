using System;
using System.Collections.Generic;

namespace Crowswood.CbusLogger
{
    internal sealed class Settings :
        ISettings
    {
        #region Members

        private readonly static Lazy<ISettings> instance = new(() => new Settings());

        #endregion

        #region Static singleton properties

        public static ISettings Instance => instance.Value;

        #endregion

        #region Properties

        public bool LoggingCbus { get; set; } = true;
        public bool LoggingGridConnect { get; set; } = true;
        public bool LoggingSerial { get; set; } = true;

        public List<string> OpCodes { get; } = new();

        public int PortNumber { get; set; } = 1;

        #endregion

        #region Constructors

        private Settings() { }

        #endregion
    }
}

using System.Collections.Generic;

namespace Crowswood.CbusLogger
{
    internal interface ISettings
    {
        bool LoggingCbus { get; set; }

        bool LoggingGridConnect { get; set; }

        bool LoggingSerial { get; set; }

        List<string> OpCodes { get; }

        int PortNumber { get; set; }
    }
}

using System;

namespace Crowswood.CbusLogger
{
    public class SerialProcessorEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
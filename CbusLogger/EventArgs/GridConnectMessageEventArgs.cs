using System;

namespace Crowswood.CbusLogger
{
    internal class GridConnectMessageEventArgs : EventArgs
    {
        public string Message { get; set; }

        public byte[] Payload { get; set; }
    }
}

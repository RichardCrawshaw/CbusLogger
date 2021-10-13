using System;

namespace Crowswood.CbusLogger
{
    internal interface ICbusProcessor :
        IDisposable
    {
        bool IsConnected { get; }

        bool IsDisposed { get; }

        void Connect();

        void Disconnect();
    }
}

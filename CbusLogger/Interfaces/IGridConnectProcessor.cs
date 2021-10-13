using System;

namespace Crowswood.CbusLogger
{
    internal interface IGridConnectProcessor :
        IDisposable
    {
        #region Properties

        bool IsDisposed { get; }

        bool IsConnected { get; }

        #endregion

        #region Methods

        bool Connect(int portNumber);

        bool Disconnect();

        #endregion

        #region Events

        event EventHandler<GridConnectMessageEventArgs> MessageReceived;

        #endregion
    }
}

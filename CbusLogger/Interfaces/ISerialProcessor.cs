using System;

namespace Crowswood.CbusLogger
{
    internal interface ISerialProcessor :
        IDisposable
    {
        #region Properties

        bool IsDisposed { get; }

        bool IsOpen { get; }

        #endregion

        #region Methods

        void Close();

        bool Open(string comPort);

        #endregion

        #region Events

        event EventHandler<SerialProcessorEventArgs> MessageReceived;

        #endregion
    }
}

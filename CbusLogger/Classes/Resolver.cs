using System;
using Unity;

namespace Crowswood.CbusLogger
{
    class Resolver
    {
        #region Members

        private static readonly Lazy<Resolver> instance = new(() => new Resolver());

        private readonly UnityContainer container = new();

        #endregion

        #region Properties

        public static Resolver Instance => instance.Value;

        #endregion

        #region Constructors

        private Resolver()
        {
            this.container.RegisterInstance<ISettings>(Settings.Instance);

            this.container.RegisterType<ISerialPortAdapter, LoggingSerialPortAdapter>();
            this.container.RegisterType<IGridConnectProcessor, LoggingGridConnectProcessor>();
            this.container.RegisterType<ICbusProcessor, LoggingCbusProcessor>();
        }

        #endregion

        #region Methods

        public T Resolve<T>() => this.container.Resolve<T>();

        #endregion
    }
}

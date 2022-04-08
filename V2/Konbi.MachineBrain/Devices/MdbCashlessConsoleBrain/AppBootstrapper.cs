using System;
using System.Collections.Generic;
using Autofac;

namespace MdbCashlessBrain
{
    public class AppBootstrapper 
    {

        public AppBootstrapper()
        {
            Initialize();
        }

        private IContainer container;

        protected void Initialize()
        {
            Configure();
        }
        protected void Configure()
        {
            var builder = new ContainerBuilder();

            //builder.RegisterType<RFIDTableService>().PropertiesAutowired().SingleInstance();
            //builder.RegisterType<SerialPortHandler>().PropertiesAutowired().SingleInstance();
            //builder.RegisterType<Services.LogService>().As(typeof(ILogService)).PropertiesAutowired().SingleInstance();
            //builder.RegisterType<NsqMessageProducerService>().As(typeof(IMessageProducerService)).PropertiesAutowired().SingleInstance();


            container = builder.Build();
        }
        public object GetInstance(Type service, string key)
        {
            return string.IsNullOrWhiteSpace(key) ?
                container.Resolve(service) :
                container.ResolveNamed(key, service);

        }

        protected IEnumerable<object> GetAllInstances(Type service)
        {
            return container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected void BuildUp(object instance)
        {
            container.InjectProperties(instance);
        }
        ~AppBootstrapper()
        {
            container.Dispose();
        }
    }
}
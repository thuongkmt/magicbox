using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Data.Test
{
    using Autofac;
    using Konbini.RfidFridge.Service.Core;
    using Konbini.RfidFridge.Service.Data;
    using Konbini.RfidFridge.Service.Util;

    public static class AutofacConfig
    {
        
        public static IContainer Config()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Application>().AsSelf().SingleInstance();

            builder.RegisterType<MachineService>().As<IMachineService>().SingleInstance();
            builder.RegisterType<LogService>().AsSelf().SingleInstance();
            builder.RegisterType<FridgeLockInterface>().AsSelf().SingleInstance(); ;
            builder.RegisterType<TemperatureInterface>().AsSelf().SingleInstance(); ;
            

            return builder.Build();
        }
    }
}

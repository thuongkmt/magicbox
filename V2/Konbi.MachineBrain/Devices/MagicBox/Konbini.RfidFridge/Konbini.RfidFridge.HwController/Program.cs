using Konbini.RfidFridge.HwController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidFridge
{
    using Autofac;
    using System.Web.Http;

    using Konbini.RfidFridge.Service.Core;

    class Program
    {
        static void Main(string[] args)
        {
            var container = AutofacConfig.Config();
            using (var scope = container.BeginLifetimeScope())
            {

                var startupType = string.Empty;
                var app = scope.Resolve<Application>();
                app.Run();

                //var startupType = string.Empty;
                //var app = scope.Resolve<TestApplication>();
                //app.Run();
            }
        }
    }
}

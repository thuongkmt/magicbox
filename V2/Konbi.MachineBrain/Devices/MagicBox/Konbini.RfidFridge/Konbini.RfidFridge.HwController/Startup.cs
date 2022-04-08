using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

[assembly: OwinStartup(typeof(Konbini.RfidFridge.HwController.Startup))]
namespace Konbini.RfidFridge.HwController
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { id = RouteParameter.Optional }
            );

            //nfigureOAuth(appBuilder);
            //appBuilder.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            appBuilder.UseWebApi(config);


            //var builder = new ContainerBuilder();
            //builder.RegisterApiControllers(Assembly.GetExecutingAssembly());


            //var assembly = typeof(Service.Type).Assembly;

            //// Core
            //builder.RegisterAssemblyTypes(assembly)
            //    .Where(type => ((
            //                        type.Namespace.EndsWith(nameof(Service.Core)) ||
            //                        type.Namespace.EndsWith(nameof(Service.Util))) && type.IsClass))
            //    .AsSelf().SingleInstance();

            //// Data
            //builder.RegisterAssemblyTypes(assembly)
            //    .Where(type => ((type.Namespace.EndsWith(nameof(Service.Data))) && type.IsClass))
            //    .AsImplementedInterfaces().InstancePerLifetimeScope();


            //// Create and assign a dependency resolver for Web API to use.
            //var container = builder.Build();
            //config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            //appBuilder.UseAutofacMiddleware(container);
            //appBuilder.UseAutofacWebApi(config);
            //appBuilder.UseWebApi(config);
        }
    }
}

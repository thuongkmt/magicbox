//namespace Konbini.RfidFridge.Web.Admin
//{
//    using Autofac;
//    using Autofac.Integration.WebApi;
//    using System.Linq;
//    using System.Reflection;
//    using System.Web.Http;
//    using Konbini.RfidFridge.Service.Data;

//    public static class AutofacConfig
//    {

//        public static IContainer Config()
//        {
//            var builder = new ContainerBuilder();

//            //Register your Web API controllers.  
//            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

//            // Service assembly
//            var assembly = typeof(Service.Type).Assembly;

//            builder.RegisterType<ProductService>().As<IProductService>();

//            // Data
//            //builder.RegisterAssemblyTypes(assembly)
//            //    .Where(type => ((type.Namespace.EndsWith(nameof(Service.Data))) && type.IsClass))
//            //    .AsImplementedInterfaces().InstancePerLifetimeScope();

//            return builder.Build();
//        }
//    }
//}

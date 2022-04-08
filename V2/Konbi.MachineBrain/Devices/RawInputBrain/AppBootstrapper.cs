using Autofac;
using Caliburn.Micro;
using Caliburn.Micro.Autofac;
using Konbi.Common.Interfaces;
using KonbiBrain.Common.Services;
using KonbiBrain.Interfaces;
using RawInputBrain.ViewModels;

namespace RawInputBrain
{
    public class AppBootstrapper : AutofacBootstrapper<ShellViewModel>
    {

        public AppBootstrapper()
        {
            Initialize();
        }


        protected override void Configure()
        {
            base.Configure();
            ConfigureTypeMapping();
        }


        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();

        }

        public static void ConfigureTypeMapping()
        {
            //Override the default subnamespaces
            var config = new TypeMappingConfiguration
            {
                DefaultSubNamespaceForViewModels = "ViewModels",
                DefaultSubNamespaceForViews = "Views"
            };

            ViewLocator.ConfigureTypeMappings(config);
            ViewModelLocator.ConfigureTypeMappings(config);
        }

        /// <summary>
        /// Register all services here
        /// </summary>
        /// <param name="container"></param>
        public static void ConfigureServices(ContainerBuilder builder)
        {

            builder.RegisterType<LogService>().As<IKonbiBrainLogService>().PropertiesAutowired().SingleInstance();

            builder.RegisterType<RawInputInterface>()
                .PropertiesAutowired()
                .SingleInstance();

        

            builder.RegisterType<NsqMessageProducerService>()
                .As<IMessageProducerService>()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();


        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            ConfigureServices(builder);
        }
    }
}
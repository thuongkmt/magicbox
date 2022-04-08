using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Caliburn.Micro.Autofac
{
    public class AutofacBootstrapper<TRootViewModel> : BootstrapperBase
    {
        public AutofacBootstrapper() : base(true)
        {
        }
        public bool AutoSubscribeEventAggegatorHandlers
        {
            get;
            set;
        }

        protected IContainer Container
        {
            get;
            private set;
        }

        public Func<IEventAggregator> CreateEventAggregator
        {
            get;
            set;
        }

        public Func<IWindowManager> CreateWindowManager
        {
            get;
            set;
        }

        public bool EnforceNamespaceConvention
        {
            get;
            set;
        }

        public Type ViewModelBaseType
        {
            get;
            set;
        }



        protected override void BuildUp(object instance)
        {
            this.Container.InjectProperties<object>(instance);
        }

        protected override void Configure()
        {
            this.ConfigureBootstrapper();
            if (this.CreateWindowManager == null)
            {
                throw new ArgumentNullException("CreateWindowManager");
            }
            if (this.CreateEventAggregator == null)
            {
                throw new ArgumentNullException("CreateEventAggregator");
            }
            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray<Assembly>()).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Type type) => type.Name.EndsWith("ViewModel")).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Type type) =>
            {
                if (!this.EnforceNamespaceConvention)
                {
                    return true;
                }
                if (string.IsNullOrWhiteSpace(type.Namespace))
                {
                    return false;
                }
                return type.Namespace.EndsWith("ViewModels");
            }).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Type type) => type.GetInterface(this.ViewModelBaseType.Name, false) != null).AsSelf<object>().InstancePerDependency().PropertiesAutowired();
            containerBuilder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray<Assembly>()).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Type type) => type.Name.EndsWith("View")).Where<object, ScanningActivatorData, DynamicRegistrationStyle>((Type type) =>
            {
                if (!this.EnforceNamespaceConvention)
                {
                    return true;
                }
                if (string.IsNullOrWhiteSpace(type.Namespace))
                {
                    return false;
                }
                return type.Namespace.EndsWith("Views");
            }).AsSelf<object>().InstancePerDependency();
            containerBuilder.Register<IWindowManager>((IComponentContext c) => this.CreateWindowManager()).InstancePerLifetimeScope().PropertiesAutowired();
            containerBuilder.Register<IEventAggregator>((IComponentContext c) => this.CreateEventAggregator()).InstancePerLifetimeScope().PropertiesAutowired();
            if (this.AutoSubscribeEventAggegatorHandlers)
            {
                containerBuilder.RegisterModule<EventAggregationAutoSubscriptionModule>();
            }
            this.ConfigureContainer(containerBuilder);
            this.Container = containerBuilder.Build(ContainerBuildOptions.None);
            this.RegisterHandlers();
         
        }

        protected virtual void RegisterHandlers()
        {

        }
        protected virtual void ConfigureBootstrapper()
        {
            this.EnforceNamespaceConvention = true;
            this.AutoSubscribeEventAggegatorHandlers = true;
            this.ViewModelBaseType = typeof(System.ComponentModel.INotifyPropertyChanged);
            this.CreateWindowManager = () => new WindowManager();
            this.CreateEventAggregator = () => new EventAggregator();
        }

        protected virtual void ConfigureContainer( ContainerBuilder builder)
        {
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return this.Container.Resolve(typeof(IEnumerable<>).MakeGenericType(new Type[] { service })) as IEnumerable<object>;
        }

        protected override object GetInstance(Type service, string key)
        {
            object obj;
            if (string.IsNullOrWhiteSpace(key))
            {
                if (this.Container.TryResolve(service, out obj))
                {
                    return obj;
                }
            }
            else if (this.Container.TryResolveNamed(key, service, out obj))
            {
                return obj;
            }
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", key ?? service.Name));
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            this.Container.Dispose();
        }
    }
    
   
}
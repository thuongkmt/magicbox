using Abp.AutoMapper;
using Abp.Dependency;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using KonbiCloud.Authorization;
using KonbiCloud.BackgroundJobs;
using KonbiCloud.Configuration;
using KonbiCloud.Messaging;
using Konbini.Messages.Services;

namespace KonbiCloud
{
    /// <summary>
    /// Application layer module of the application.
    /// </summary>
    [DependsOn(
        typeof(KonbiCloudCoreModule)
        )]
    public class KonbiCloudApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            //Adding authorization providers
            Configuration.Authorization.Providers.Add<AppAuthorizationProvider>();

            //Adding custom AutoMapper configuration
            Configuration.Modules.AbpAutoMapper().Configurators.Add(CustomDtoMapper.CreateMappings);
        }

        public override void Initialize()
        {
            IocManager.Register<IConnectToRabbitMqMessageService, ConnectToRabbitMqService>(DependencyLifeStyle.Singleton);
            //IocManager.Register<ISendMessageToCloudService, RabbitMqSendMessageToCloudService>();
            IocManager.Register<ISendMessageToMachineClientService, RabbitMqSendMessageToMachineService>(DependencyLifeStyle.Singleton);


            var _configurationRoot = IocManager.Resolve<IAppConfigurationAccessor>().Configuration;
            var hostName = _configurationRoot["RabbitMQ:HostName"];
            var userName = _configurationRoot["RabbitMQ:UserName"];
            var pwd = _configurationRoot["RabbitMQ:Password"];
            var sendToMachineSvc = IocManager.Resolve<ISendMessageToMachineClientService>();
            sendToMachineSvc.InitConfigAndConnect(hostName,userName,pwd);

            IocManager.RegisterAssemblyByConvention(typeof(KonbiCloudApplicationModule).GetAssembly());
            //IocManager.Register<ITopupMessageHandler>(DependencyLifeStyle.Transient);
        }

        public override void PostInitialize()
        {
            var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
           
            //if(isListeningRabbitMq)
            workManager.Add(IocManager.Resolve<RabbitMqListenerJob>());
            //notify by slack
            workManager.Add(IocManager.Resolve<NotifyStatusMachineBySlack>());

        }

    }
}
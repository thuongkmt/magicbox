using Abp.AutoMapper;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Modules;
using Abp.Reflection.Extensions;
using KonbiCloud.Authorization;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.DeviceSettings;
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
            IocManager.Register<IMessageProducerService, NsqMessageProducerService>(DependencyLifeStyle.Transient);
            IocManager.Register<IBillAcceptorHanlderService, BillAcceptorHanlderService>(DependencyLifeStyle.Transient);


            IocManager.Register<IConnectToRabbitMqMessageService,ConnectToRabbitMqService>(DependencyLifeStyle.Singleton);
            IocManager.Register<ISendMessageToCloudService,RabbitMqSendMessageToCloudService>(DependencyLifeStyle.Singleton);
            //IocManager.Register<ISendMessageToMachineClientService,RabbitMqSendMessageToMachineService>();


           


            IocManager.RegisterAssemblyByConvention(typeof(KonbiCloudApplicationModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

            var sv = IocManager.Resolve<ISettingManager>();
            var machineId = sv.GetSettingValue(AppSettingNames.MachineId);
            var machineName = sv.GetSettingValue(AppSettingNames.MachineName);

            var hostName = sv.GetSettingValue(AppSettingNames.RabbitMqServer);
            var userName = sv.GetSettingValue(AppSettingNames.RabbitMqUser);
            var pwd = sv.GetSettingValue(AppSettingNames.RabbitMqPassword);

            var mName = machineName?.Replace(" ", string.Empty);
            var clientName = $"SendToCloudJob-{mName}-{machineId}";

            var sendToCloudSvc = IocManager.Resolve<ISendMessageToCloudService>();
            sendToCloudSvc.InitConfigAndConnect(hostName, userName, pwd, clientName);
        }
    }
}
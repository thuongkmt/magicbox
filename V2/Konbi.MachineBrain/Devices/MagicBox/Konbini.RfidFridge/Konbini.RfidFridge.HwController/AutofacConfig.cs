namespace Konbini.RfidFridge.HwController
{
    using Autofac;
    using Autofac.Integration.WebApi;
    using Konbini.Messages.Services;
    using Konbini.RfidFridge.Common;
    using Konbini.RfidFridge.Domain.Enums;
    using Konbini.RfidFridge.Service.Core;
    using Konbini.RfidFridge.Service.Data;
    using Konbini.RfidFridge.Service.Devices;
    using Konbini.RfidFridge.Service.Util;
    using System;
    using System.Linq;
    using System.Reflection;

    public static class AutofacConfig
    {

        public static IContainer Config()
        {
            var builder = new ContainerBuilder();
           builder.RegisterType<Application>().AsSelf().SingleInstance();
            builder.RegisterType<TestApplication>().As<TestApplication>().SingleInstance();

            // Service assembly
            var assembly = typeof(Service.Type).Assembly;

            // Core
            builder.RegisterAssemblyTypes(assembly)
                .Where(type => ((
                type.Namespace.EndsWith(nameof(Service.Core)) ||
                type.Namespace.EndsWith(nameof(Service.Util))) && type.IsClass))
                .AsSelf().SingleInstance();

            // Data
            builder.RegisterAssemblyTypes(assembly)
                .Where(type => ((type.Namespace.EndsWith(nameof(Service.Data))) && type.IsClass))
                .AsImplementedInterfaces().InstancePerLifetimeScope();





            //register cloud rabbitmq
            builder.RegisterType<ConnectToRabbitMqService>().As<IConnectToRabbitMqMessageService>().SingleInstance();
            builder.RegisterType<RabbitMqSendMessageToCloudService>().As<ISendMessageToCloudService>().SingleInstance();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var log = new LogService();
            var web = new WebApiService(log);

            // Web Api Data
            var webApiUrl = System.Configuration.ConfigurationManager.AppSettings.Get("WebApi_Url");
            var webApiUserName = System.Configuration.ConfigurationManager.AppSettings.Get("WebApi_UserName");
            var webApiPassword = System.Configuration.ConfigurationManager.AppSettings.Get("WebApi_Password");
            web.Init(webApiUrl, webApiUserName, webApiPassword);

            // After this, depend on setting
            //var setting = new SettingService(web, log);
            //setting.GetAll().Wait();

            //var paymentType = RfidFridgeSetting.System.Payment.Type;

            //Enum.TryParse(RfidFridgeSetting.System.Payment.Magic.CardInsertType, out MagicPaymentCardInsertType cardInsertType);

            // Devices
            //if(cardInsertType == MagicPaymentCardInsertType.DEFAULT)
            //{
            //    builder.RegisterType<MagicCashlessPayment.Core.Application>().As<IFridgePayment>().SingleInstance();
            //}

            //if (cardInsertType == MagicPaymentCardInsertType.SLIM)
            //{
            //    builder.RegisterType<MagicCashlessPayment.Core.SlimCardInsertApplication>().As<IFridgePayment>().SingleInstance();
            //}

            builder.RegisterType<MagicCashlessPayment.Core.IM30Application>().As<IFridgePayment>().SingleInstance();
            var container = builder.Build();


            CurrentContainer = container;
            return container;
        }

        public static IContainer CurrentContainer { get; set; }
    }
}

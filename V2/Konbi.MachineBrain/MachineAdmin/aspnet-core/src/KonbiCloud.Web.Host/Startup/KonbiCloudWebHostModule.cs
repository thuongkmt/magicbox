using System;
using Abp.AspNetZeroCore;
using Abp.AspNetZeroCore.Web.Authentication.External;
using Abp.AspNetZeroCore.Web.Authentication.External.Facebook;
using Abp.AspNetZeroCore.Web.Authentication.External.Google;
using Abp.AspNetZeroCore.Web.Authentication.External.Microsoft;
using Abp.AspNetZeroCore.Web.Authentication.External.OpenIdConnect;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.MailKit;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using KonbiCloud.BackgroundJobs;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.EntityFrameworkCore;
using KonbiCloud.MultiTenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Abp.Configuration;

namespace KonbiCloud.Web.Startup
{
    [DependsOn(typeof(KonbiCloudWebCoreModule))]
    [DependsOn(typeof(AbpMailKitModule))]
    public class KonbiCloudWebHostModule : AbpModule
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public KonbiCloudWebHostModule(
            IHostingEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            //Configuration.Modules.AbpWebCommon().MultiTenancy.DomainFormat = _appConfiguration["App:ServerRootAddress"] ?? "http://localhost:22742/";
            Configuration.Modules.AbpWebCommon().MultiTenancy.DomainFormat = "{0}.konbi.cloud";
            Configuration.Modules.AspNetZero().LicenseCode = _appConfiguration["AbpZeroLicenseCode"];
            Configuration.ReplaceService<IMailKitSmtpBuilder, MyMailKitSmtpBuilder>();
            Configuration.ReplaceService<IEmailSender, KonbiEmailSender>();

        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(KonbiCloudWebHostModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            using (var scope = IocManager.CreateScope())
            {
                if (!scope.Resolve<DatabaseCheckHelper>().Exist(_appConfiguration["ConnectionStrings:Default"]))
                {
                    return;
                }
            }

            var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
            if (IocManager.Resolve<IMultiTenancyConfig>().IsEnabled)
            {              
                workManager.Add(IocManager.Resolve<SubscriptionExpirationCheckWorker>());
                workManager.Add(IocManager.Resolve<SubscriptionExpireEmailNotifierWorker>());
            }

            // workManager.Add(IocManager.Resolve<RfidTableNsqIncomingMessageService>());
            // workManager.Add(IocManager.Resolve<PaymentResponseNsqIncomingMessageService>());
            //workManager.Add(IocManager.Resolve<SyncDishJob>());
            workManager.Add(IocManager.Resolve<SyncTransactionJob>());

            workManager.Add(IocManager.Resolve<RabbitMqListenerJob>());
            workManager.Add(IocManager.Resolve<StopSaleMessageService>());
            workManager.Add(IocManager.Resolve<CleanTemperatureJob>());

            ConfigureExternalAuthProviders();
            var settingManager = IocManager.Resolve<ISettingManager>();
            var machineId = settingManager.GetSettingValue(AppSettingNames.MachineId);
            if (string.IsNullOrWhiteSpace(machineId)) machineId = Guid.NewGuid().ToString();
            settingManager.ChangeSettingForApplication(AppSettingNames.MachineId,machineId);

        }

        private void ConfigureExternalAuthProviders()
        {
            var externalAuthConfiguration = IocManager.Resolve<ExternalAuthConfiguration>();

            if (bool.Parse(_appConfiguration["Authentication:OpenId:IsEnabled"]))
            {
                externalAuthConfiguration.Providers.Add(
                    new ExternalLoginProviderInfo(
                        OpenIdConnectAuthProviderApi.Name,
                        _appConfiguration["Authentication:OpenId:ClientId"],
                        _appConfiguration["Authentication:OpenId:ClientSecret"],
                        typeof(OpenIdConnectAuthProviderApi),
                        new Dictionary<string, string>
                        {
                            {"Authority", _appConfiguration["Authentication:OpenId:Authority"]},
                            {"LoginUrl",_appConfiguration["Authentication:OpenId:LoginUrl"]}
                        }
                    )
                );
            }

            if (bool.Parse(_appConfiguration["Authentication:Facebook:IsEnabled"]))
            {
                externalAuthConfiguration.Providers.Add(
                    new ExternalLoginProviderInfo(
                        FacebookAuthProviderApi.Name,
                        _appConfiguration["Authentication:Facebook:AppId"],
                        _appConfiguration["Authentication:Facebook:AppSecret"],
                        typeof(FacebookAuthProviderApi)
                    )
                );
            }

            if (bool.Parse(_appConfiguration["Authentication:Google:IsEnabled"]))
            {
                externalAuthConfiguration.Providers.Add(
                    new ExternalLoginProviderInfo(
                        GoogleAuthProviderApi.Name,
                        _appConfiguration["Authentication:Google:ClientId"],
                        _appConfiguration["Authentication:Google:ClientSecret"],
                        typeof(GoogleAuthProviderApi)
                    )
                );
            }

            //not implemented yet. Will be implemented with https://github.com/aspnetzero/aspnet-zero-angular/issues/5
            if (bool.Parse(_appConfiguration["Authentication:Microsoft:IsEnabled"]))
            {
                externalAuthConfiguration.Providers.Add(
                    new ExternalLoginProviderInfo(
                        MicrosoftAuthProviderApi.Name,
                        _appConfiguration["Authentication:Microsoft:ConsumerKey"],
                        _appConfiguration["Authentication:Microsoft:ConsumerSecret"],
                        typeof(MicrosoftAuthProviderApi)
                    )
                );
            }
        }
    }
}

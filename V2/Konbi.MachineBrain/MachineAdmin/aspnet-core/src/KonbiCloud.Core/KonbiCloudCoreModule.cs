using System;
using Abp.AspNetZeroCore;
using Abp.AspNetZeroCore.Timing;
using Abp.AutoMapper;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.Reflection.Extensions;
using Abp.Timing;
using Abp.Configuration.Startup;
using Abp.MailKit;
using Abp.Net.Mail.Smtp;
using Abp.Zero;
using Abp.Zero.Configuration;
using Abp.Zero.Ldap;
using Castle.MicroKernel.Registration;
using KonbiCloud.Authorization.Roles;
using KonbiCloud.Authorization.Users;
using KonbiCloud.Chat;
using KonbiCloud.Configuration;
using KonbiCloud.Debugging;
using KonbiCloud.Emailing;
using KonbiCloud.Features;
using KonbiCloud.Friendships;
using KonbiCloud.Friendships.Cache;
using KonbiCloud.Localization;
using KonbiCloud.MultiTenancy;
using KonbiCloud.MultiTenancy.Payments.Cache;
using KonbiCloud.Notifications;

namespace KonbiCloud
{
    [DependsOn(
        typeof(AbpZeroCoreModule),
        typeof(AbpZeroLdapModule),
        typeof(AbpAutoMapperModule),
        typeof(AbpAspNetZeroCoreModule),
        typeof(AbpMailKitModule))]
    public class KonbiCloudCoreModule : AbpModule
    {
        public override void PreInitialize()
        {
            //workaround for issue: https://github.com/aspnet/EntityFrameworkCore/issues/9825
            //related github issue: https://github.com/aspnet/EntityFrameworkCore/issues/10407
            AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue9825", true);

            Configuration.Auditing.IsEnabledForAnonymousUsers = true;

            //Declare entity types
            Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
            Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
            Configuration.Modules.Zero().EntityTypes.User = typeof(User);

            KonbiCloudLocalizationConfigurer.Configure(Configuration.Localization);

            //Adding feature providers
            Configuration.Features.Providers.Add<AppFeatureProvider>();

            //Adding setting providers
            Configuration.Settings.Providers.Add<AppSettingProvider>();

            //Adding notification providers
            Configuration.Notifications.Providers.Add<AppNotificationProvider>();

            //Enable this line to create a multi-tenant application.
            Configuration.MultiTenancy.IsEnabled = KonbiCloudConsts.MultiTenancyEnabled;
            
            //Enable LDAP authentication (It can be enabled only if MultiTenancy is disabled!)
            //Configuration.Modules.ZeroLdap().Enable(typeof(AppLdapAuthenticationSource));

            //Configure roles
            AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

            if (DebugHelper.IsDebug)
            {
                //Disabling email sending in debug mode
                Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
            }

            Configuration.ReplaceService(typeof(IEmailSenderConfiguration), () =>
            {
                Configuration.IocManager.IocContainer.Register(
                    Component.For<IEmailSenderConfiguration, ISmtpEmailSenderConfiguration>()
                             .ImplementedBy<KonbiCloudSmtpEmailSenderConfiguration>()
                             .LifestyleTransient()
                );
            });

            Configuration.Caching.Configure(FriendCacheItem.CacheName, cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(30);
            });

            Configuration.Caching.Configure(PaymentCacheItem.CacheName, cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(KonbiCloudConsts.PaymentCacheDurationInMinutes);
            });
            //Dashboard cache

            Configuration.Caching.Configure("DashboardCache1_TotalTransSale", cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(10);
            });
            Configuration.Caching.Configure("DashboardCache1_TotalTransToday", cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(10);
            });
            Configuration.Caching.Configure("DashboardCache1_TotalTransCurrentSession", cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(10);
            });
            Configuration.Caching.Configure("DashboardCache1_GetTotalTransCurrentSessionSale", cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(10);
            });
            Configuration.Caching.Configure("DashboardCache2_Daily", cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(10);
            });
            Configuration.Caching.Configure("DashboardCache2_Weekly", cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(10);
            });
            Configuration.Caching.Configure("DashboardCache2_Monthly", cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(10);
            });
            Configuration.Caching.Configure("DashboardCache3", cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(10);
            });
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(KonbiCloudCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            IocManager.RegisterIfNot<IChatCommunicator, NullChatCommunicator>();

            IocManager.Resolve<ChatUserStateWatcher>().Initialize();
            IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
        }
    }
}
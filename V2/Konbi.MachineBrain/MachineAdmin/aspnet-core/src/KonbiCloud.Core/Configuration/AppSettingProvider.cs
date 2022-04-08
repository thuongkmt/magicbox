using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Configuration;
using Abp.Zero.Configuration;
using KonbiCloud.Configuration;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Konbini.RfidFridge.Common;
using System.ComponentModel;

namespace KonbiCloud.Configuration
{
    /// <summary>
    /// Defines settings for the application.
    /// See <see cref="AppSettings"/> for setting names.
    /// </summary>
    public class AppSettingProvider : SettingProvider
    {
        private readonly IConfigurationRoot _appConfiguration;

        public AppSettingProvider(IAppConfigurationAccessor configurationAccessor)
        {
            _appConfiguration = configurationAccessor.Configuration;
        }

        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            //Disable TwoFactorLogin by default (can be enabled by UI)
            context.Manager.GetSettingDefinition(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEnabled).DefaultValue = false.ToString().ToLowerInvariant();

            return GetHostSettings().Union(GetTenantSettings()).Union(GetSharedSettings())
            .Union(GetRfidFridgeSettings());
        }

        private IEnumerable<SettingDefinition> GetHostSettings()
        {
            return new[] {
                new SettingDefinition(AppSettings.TenantManagement.AllowSelfRegistration, GetFromAppSettings(AppSettings.TenantManagement.AllowSelfRegistration, "true"), isVisibleToClients: true),
                new SettingDefinition(AppSettings.TenantManagement.IsNewRegisteredTenantActiveByDefault, GetFromAppSettings(AppSettings.TenantManagement.IsNewRegisteredTenantActiveByDefault, "false")),
                new SettingDefinition(AppSettings.TenantManagement.UseCaptchaOnRegistration, GetFromAppSettings(AppSettings.TenantManagement.UseCaptchaOnRegistration, "true"), isVisibleToClients: true),
                new SettingDefinition(AppSettings.TenantManagement.DefaultEdition, GetFromAppSettings(AppSettings.TenantManagement.DefaultEdition, "")),
                new SettingDefinition(AppSettings.UserManagement.SmsVerificationEnabled, GetFromAppSettings(AppSettings.UserManagement.SmsVerificationEnabled, "false"), isVisibleToClients: true),
                new SettingDefinition(AppSettings.TenantManagement.SubscriptionExpireNotifyDayCount, GetFromAppSettings(AppSettings.TenantManagement.SubscriptionExpireNotifyDayCount, "7"), isVisibleToClients: true),
                new SettingDefinition(AppSettings.HostManagement.BillingLegalName, GetFromAppSettings(AppSettings.HostManagement.BillingLegalName, "")),
                new SettingDefinition(AppSettings.HostManagement.BillingAddress, GetFromAppSettings(AppSettings.HostManagement.BillingAddress, "")),
                new SettingDefinition(AppSettings.Recaptcha.SiteKey, GetFromSettings("Recaptcha:SiteKey"), isVisibleToClients: true),
                new SettingDefinition(AppSettings.UiManagement.LayoutType, GetFromAppSettings(AppSettings.UiManagement.LayoutType, "fluid"), isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.ContentSkin, GetFromAppSettings(AppSettings.UiManagement.ContentSkin, "light2"), isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.Header.DesktopFixedHeader, GetFromAppSettings(AppSettings.UiManagement.Header.DesktopFixedHeader, "true"),isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.Header.MobileFixedHeader, GetFromAppSettings(AppSettings.UiManagement.Header.MobileFixedHeader, "false"),isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.Header.Skin, GetFromAppSettings(AppSettings.UiManagement.Header.Skin, "light"),isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.LeftAside.Position, GetFromAppSettings(AppSettings.UiManagement.LeftAside.Position, "left"), isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.LeftAside.AsideSkin, GetFromAppSettings(AppSettings.UiManagement.LeftAside.AsideSkin, "light"), isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.LeftAside.FixedAside, GetFromAppSettings(AppSettings.UiManagement.LeftAside.FixedAside, "true"),isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.LeftAside.AllowAsideMinimizing, GetFromAppSettings(AppSettings.UiManagement.LeftAside.AllowAsideMinimizing, "true"),isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.LeftAside.DefaultMinimizedAside, GetFromAppSettings(AppSettings.UiManagement.LeftAside.DefaultMinimizedAside, "false"),isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.LeftAside.AllowAsideHiding, GetFromAppSettings(AppSettings.UiManagement.LeftAside.AllowAsideHiding, "false"),isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.LeftAside.DefaultHiddenAside, GetFromAppSettings(AppSettings.UiManagement.LeftAside.DefaultHiddenAside, "false"),isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.Footer.FixedFooter, GetFromAppSettings(AppSettings.UiManagement.Footer.FixedFooter, "false"),isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.Theme, GetFromAppSettings(AppSettings.UiManagement.Theme, "default"), isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettings.UiManagement.ThemeColor, GetFromAppSettings(AppSettings.UiManagement.ThemeColor, "default"), isVisibleToClients: true, scopes: SettingScopes.All),
                new SettingDefinition(AppSettingNames.UiTheme, "red", scopes: SettingScopes.Application | SettingScopes.Tenant | SettingScopes.User, isVisibleToClients: true),
                new SettingDefinition(AppSettingNames.Currency, "SGD", scopes: SettingScopes.Tenant| SettingScopes.User|SettingScopes.All, isVisibleToClients: true),
                new SettingDefinition(AppSettingNames.CloudName, "Konbini", scopes: SettingScopes.Tenant| SettingScopes.User|SettingScopes.All, isVisibleToClients: true),
                new SettingDefinition(AppSettingNames.CurrencySymbol, "$", scopes: SettingScopes.Tenant| SettingScopes.User|SettingScopes.All, isVisibleToClients: true),
                new SettingDefinition(AppSettingNames.ShowEmployeeManagement, "False", scopes: SettingScopes.All, isVisibleToClients: true),
                new SettingDefinition(AppSettingNames.MachineId,"", scopes: SettingScopes.All, isVisibleToClients: true),
                new SettingDefinition(AppSettingNames.MachineName, "New-Machine", scopes: SettingScopes.All, isVisibleToClients: true),
                new SettingDefinition(AppSettingNames.SyncServerUrl, "http://localhost:22742", scopes: SettingScopes.All, isVisibleToClients: true),
                new SettingDefinition(AppSettingNames.MdbCashlessPort, "COM1", scopes: SettingScopes.All, isVisibleToClients: true),
                new SettingDefinition(AppSettingNames.AllowPushDishToServer, "False", scopes: SettingScopes.All, isVisibleToClients: true),
                new SettingDefinition(AppSettingNames.ScanningStableAfterMiliseconds, "2000", scopes: SettingScopes.All, isVisibleToClients: true),
                new SettingDefinition(AppSettingNames.PreventSellingSamePlateInASession, "False", scopes: SettingScopes.All, isVisibleToClients: true)
            };
        }

        private IEnumerable<SettingDefinition> GetTenantSettings()
        {
            return new[]
            {
                new SettingDefinition(AppSettings.UserManagement.AllowSelfRegistration, GetFromAppSettings(AppSettings.UserManagement.AllowSelfRegistration, "true"), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.UserManagement.IsNewRegisteredUserActiveByDefault, GetFromAppSettings(AppSettings.UserManagement.IsNewRegisteredUserActiveByDefault, "false"), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.UserManagement.UseCaptchaOnRegistration, GetFromAppSettings(AppSettings.UserManagement.UseCaptchaOnRegistration, "true"), scopes: SettingScopes.Tenant, isVisibleToClients: true),
                new SettingDefinition(AppSettings.TenantManagement.BillingLegalName, GetFromAppSettings(AppSettings.TenantManagement.BillingLegalName, ""), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.BillingAddress, GetFromAppSettings(AppSettings.TenantManagement.BillingAddress, ""), scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.BillingTaxVatNo, GetFromAppSettings(AppSettings.TenantManagement.BillingTaxVatNo, ""), scopes: SettingScopes.Tenant)
            };
        }

        private IEnumerable<SettingDefinition> GetSharedSettings()
        {
            return new[]
            {
                new SettingDefinition(AppSettings.UserManagement.TwoFactorLogin.IsGoogleAuthenticatorEnabled, GetFromAppSettings(AppSettings.UserManagement.TwoFactorLogin.IsGoogleAuthenticatorEnabled, "false"), scopes: SettingScopes.Application | SettingScopes.Tenant, isVisibleToClients: true)
            };
        }

        private IEnumerable<SettingDefinition> GetRfidFridgeSettings()
        {
            var settingDefinition = new List<SettingDefinition>();

            var settings = Assembly
                .GetAssembly(typeof(RfidFridgeSetting))
                .GetTypes();

            foreach (var setting in settings)
            {
                var properties = setting.GetProperties();
                if (properties.Length > 0)
                {

                    var settingKey = setting.FullName.Replace(setting.Namespace, string.Empty);
                    settingKey = settingKey.Substring(1, settingKey.Length - 1).Replace("+", ".");

                    foreach (var propertyInfo in properties)
                    {
                        var key = $"{settingKey}.{propertyInfo.Name}";
                        var defaultValueAttributes = propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), true);
                        var defaultValue = string.Empty;
                        if (defaultValueAttributes.Length > 0)
                        {
                            defaultValue = Convert.ToString(((DefaultValueAttribute)defaultValueAttributes[0]).Value);
                        }
                        else
                        {
                            defaultValue = $"[{propertyInfo.Name}]";
                        }

                        settingDefinition.Add(new SettingDefinition(key, defaultValue, scopes: SettingScopes.All, isVisibleToClients: true));
                    }
                }

            }
            return settingDefinition;
        }

        private string GetFromAppSettings(string name, string defaultValue = null)
        {
            return GetFromSettings("App:" + name, defaultValue);
        }

        private string GetFromSettings(string name, string defaultValue = null)
        {
            return _appConfiguration[name] ?? defaultValue;
        }
    }
}

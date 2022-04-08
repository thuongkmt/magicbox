import { UtilsService } from '@abp/utils/utils.service';
import { CompilerOptions, NgModuleRef, Type } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppAuthService } from '@app/shared/common/auth/app-auth.service';
import { AppConsts } from '@shared/AppConsts';
import { SubdomainTenancyNameFinder } from '@shared/helpers/SubdomainTenancyNameFinder';
import * as moment from 'moment';
import * as _ from 'lodash';
import { DynamicResourcesHelper } from './shared/helpers/DynamicResourcesHelper';
import { UrlHelper } from './shared/helpers/UrlHelper';
import { XmlHttpRequestHelper } from '@shared/helpers/XmlHttpRequestHelper';
import { environment } from './environments/environment';

export class AppPreBootstrap {

    static run(appRootUrl: string, callback: () => void, resolve: any, reject: any): void {
        AppPreBootstrap.getApplicationConfig(appRootUrl, () => {
            if (UrlHelper.isInstallUrl(location.href)) {
                DynamicResourcesHelper.loadStlyes();
                callback();
                return;
            }

            const queryStringObj = UrlHelper.getQueryParameters();

            if (queryStringObj.redirect && queryStringObj.redirect === 'TenantRegistration') {
                if (queryStringObj.forceNewRegistration) {
                    new AppAuthService().logout();
                }

                location.href = AppConsts.appBaseUrl + '/account/select-edition';
            } else if (queryStringObj.impersonationToken) {
                AppPreBootstrap.impersonatedAuthenticate(queryStringObj.impersonationToken, queryStringObj.tenantId, () => { AppPreBootstrap.getUserConfiguration(callback); });
            } else if (queryStringObj.switchAccountToken) {
                AppPreBootstrap.linkedAccountAuthenticate(queryStringObj.switchAccountToken, queryStringObj.tenantId, () => { AppPreBootstrap.getUserConfiguration(callback); });
            } else {
                AppPreBootstrap.getUserConfiguration(callback);
            }

            //console.log(AppConsts.remoteServiceBaseUrl);
            //Ha - set current tenant based on url
            XmlHttpRequestHelper.ajax(
                'GET',
                AppConsts.remoteServiceBaseUrl + '/api/services/app/Tenant/GetTenantIdByHostName?hostName=' + window.location.hostname,
                null,
                null,
                (response) => {
                    let result = response.result;
                    if (result > 0) {
                        abp.multiTenancy.setTenantIdCookie(result);
                    }
                }
            );
        });


    }

    static bootstrap<TM>(moduleType: Type<TM>, compilerOptions?: CompilerOptions | CompilerOptions[]): Promise<NgModuleRef<TM>> {
        return platformBrowserDynamic().bootstrapModule(moduleType, compilerOptions);
    }

    private static getApplicationConfig(appRootUrl: string, callback: () => void) {
        let type = 'GET';
        let url = appRootUrl + 'assets/' + environment.appConfig;
        let customHeaders = [
            {
                name: 'Abp.TenantId',
                value: abp.multiTenancy.getTenantIdCookie() + ''
            }];

        XmlHttpRequestHelper.ajax(type, url, customHeaders, null, (result) => {
            const subdomainTenancyNameFinder = new SubdomainTenancyNameFinder();
            const tenancyName = subdomainTenancyNameFinder.getCurrentTenancyNameOrNull(result.appBaseUrl);


            let appBaseUrl = result.appBaseUrl;
            let remoteServiceBaseUrl = result.remoteServiceBaseUrl;

            let appBasePort = '';
            let appBaseUrlPart = appBaseUrl.split(':');
            console.log(appBaseUrlPart);
            if(appBaseUrlPart.length > 1)
            {
                // Port
                appBasePort = appBaseUrlPart[1];
            }    

            let remoteServiceBasePort = '';
            let remoteServiceBaseUrlPart = remoteServiceBaseUrl.split(':');
            if(remoteServiceBaseUrlPart.length > 1)
            {
                // Port
                remoteServiceBasePort = remoteServiceBaseUrlPart[1];
            }      


            if (appBaseUrl.startsWith('root')) {
                appBaseUrl = appRootUrl;
            }

            if (remoteServiceBaseUrl.startsWith('root')) {
                console.log('Root URL:' + appRootUrl);
                console.log('App Base Port:' + appBasePort);

                if(appBasePort)
                {
                    remoteServiceBaseUrl = appRootUrl.substr(0, appRootUrl.length-1).replace(':' + appBasePort , '').concat(':'+ remoteServiceBasePort);
                }else {
                    remoteServiceBaseUrl = appRootUrl.substr(0, appRootUrl.length-1).concat(':'+ remoteServiceBasePort);
                }
            }

      
            AppConsts.appBaseUrlFormat = appBaseUrl;
            AppConsts.remoteServiceBaseUrlFormat = remoteServiceBaseUrl;
            AppConsts.localeMappings = result.localeMappings;

            console.log('Base URL:' + appBaseUrl);
            console.log('Server URL:' + remoteServiceBaseUrl);

            if (tenancyName == null) {
                AppConsts.appBaseUrl = appBaseUrl.replace(AppConsts.tenancyNamePlaceHolderInUrl + '.', '');
                AppConsts.remoteServiceBaseUrl = remoteServiceBaseUrl.replace(AppConsts.tenancyNamePlaceHolderInUrl + '.', '');
            } else {
                AppConsts.appBaseUrl = appBaseUrl.replace(AppConsts.tenancyNamePlaceHolderInUrl, tenancyName);
                AppConsts.remoteServiceBaseUrl = remoteServiceBaseUrl.replace(AppConsts.tenancyNamePlaceHolderInUrl, tenancyName);
            }


            callback();
        });
    }

    private static getCurrentClockProvider(currentProviderName: string): abp.timing.IClockProvider {
        if (currentProviderName === 'unspecifiedClockProvider') {
            return abp.timing.unspecifiedClockProvider;
        }

        if (currentProviderName === 'utcClockProvider') {
            return abp.timing.utcClockProvider;
        }

        return abp.timing.localClockProvider;
    }

    private static impersonatedAuthenticate(impersonationToken: string, tenantId: number, callback: () => void): void {
        abp.multiTenancy.setTenantIdCookie(tenantId);
        const cookieLangValue = abp.utils.getCookieValue('Abp.Localization.CultureName');

        let requestHeaders = {
            '.AspNetCore.Culture': ('c=' + cookieLangValue + '|uic=' + cookieLangValue),
            'Abp.TenantId': abp.multiTenancy.getTenantIdCookie()
        };

        XmlHttpRequestHelper.ajax(
            'POST',
            AppConsts.remoteServiceBaseUrl + '/api/TokenAuth/ImpersonatedAuthenticate?impersonationToken=' + impersonationToken,
            requestHeaders,
            null,
            (response) => {
                let result = response.result;
                abp.auth.setToken(result.accessToken);
                AppPreBootstrap.setEncryptedTokenCookie(result.encryptedAccessToken);
                location.search = '';
                callback();
            }
        );
    }

    private static linkedAccountAuthenticate(switchAccountToken: string, tenantId: number, callback: () => void): void {
        abp.multiTenancy.setTenantIdCookie(tenantId);
        const cookieLangValue = abp.utils.getCookieValue('Abp.Localization.CultureName');

        let requestHeaders = {
            '.AspNetCore.Culture': ('c=' + cookieLangValue + '|uic=' + cookieLangValue),
            'Abp.TenantId': abp.multiTenancy.getTenantIdCookie()
        };

        XmlHttpRequestHelper.ajax(
            'POST',
            AppConsts.remoteServiceBaseUrl + '/api/TokenAuth/LinkedAccountAuthenticate?switchAccountToken=' + switchAccountToken,
            requestHeaders,
            null,
            (response) => {
                let result = response.result;
                abp.auth.setToken(result.accessToken);
                AppPreBootstrap.setEncryptedTokenCookie(result.encryptedAccessToken);
                location.search = '';
                callback();
            }
        );
    }

    private static getUserConfiguration(callback: () => void): any {
        const cookieLangValue = abp.utils.getCookieValue('Abp.Localization.CultureName');
        const token = abp.auth.getToken();

        let requestHeaders = {
            '.AspNetCore.Culture': ('c=' + cookieLangValue + '|uic=' + cookieLangValue),
            'Abp.TenantId': abp.multiTenancy.getTenantIdCookie()
        };

        if (token) {
            requestHeaders['Authorization'] = 'Bearer ' + token;
        }

        return XmlHttpRequestHelper.ajax('GET', AppConsts.remoteServiceBaseUrl + '/AbpUserConfiguration/GetAll', requestHeaders, null, (response) => {
            let result = response.result;

            _.merge(abp, result);

            abp.clock.provider = this.getCurrentClockProvider(result.clock.provider);

            moment.locale(abp.localization.currentLanguage.name);
            (window as any).moment.locale(abp.localization.currentLanguage.name);

            if (abp.clock.provider.supportsMultipleTimezone) {
                moment.tz.setDefault(abp.timing.timeZoneInfo.iana.timeZoneId);
                (window as any).moment.tz.setDefault(abp.timing.timeZoneInfo.iana.timeZoneId);
            }

            abp.event.trigger('abp.dynamicScriptsInitialized');

            AppConsts.recaptchaSiteKey = abp.setting.get('Recaptcha.SiteKey');
            AppConsts.subscriptionExpireNootifyDayCount = parseInt(abp.setting.get('App.TenantManagement.SubscriptionExpireNotifyDayCount'));

            DynamicResourcesHelper.loadResources(callback);
        });
    }

    private static setEncryptedTokenCookie(encryptedToken: string) {
        new UtilsService().setCookieValue(AppConsts.authorization.encrptedAuthTokenName,
            encryptedToken,
            new Date(new Date().getTime() + 365 * 86400000), //1 year
            abp.appPath
        );
    }
}

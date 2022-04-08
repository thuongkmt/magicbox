import { UrlHelper } from './shared/UrlHelper';
import { AppConsts } from './AppConsts';
import { XmlHttpRequestHelper } from './shared/XmlHttpRequestHelper';

export class AppPreBootstrap {
    static run(appRootUrl: string,  callback: () => void,resolve: any, reject: any): void {
        AppPreBootstrap.getApplicationConfig(appRootUrl,()=>{
            callback();
        });
    }

    private static getApplicationConfig(appRootUrl: string, callback: () => void):void {
        let type = 'GET';
        let url = appRootUrl + 'assets/appconfig.json';
        let customHeaders = [
            {
                name: 'konbiwalletapp'
            }];

        XmlHttpRequestHelper.ajax(type, url, customHeaders, null, (result) => {
           
            console.log(result);
            AppConsts.magicBoxBackendurl = result.magicBoxBackendurl;
            AppConsts.webAdminBackendUrl = result.webAdminBackendUrl;
            AppConsts.rabbitMqStompUrl=result.rabbitMqStompUrl;
            AppConsts.rabbitMqUser=result.rabbitMqUser;
            AppConsts.rabbitMqUserPwd=result.rabbitMqUserPwd;
            callback();

        });
      

    }


}
import { AppConsts } from '@shared/AppConsts';
import * as rtlDetect from 'rtl-detect';
import { StyleLoaderService } from '@shared/utils/style-loader.service';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';
import { Demo8ThemeAssetContributor } from '@app/shared/layout/themes/demo8/Demo8ThemeAssetContributor';
import { DefaultThemeAssetContributor } from '@app/shared/layout/themes/default/DefaultThemeAssetContributor';
import * as _ from 'lodash';

export class DynamicResourcesHelper {

    static loadResources(callback: () => void): void {
        Promise.all([DynamicResourcesHelper.loadStlyes()])
            .then(() => {
                callback();
            });
    }

    static loadStlyes(): Promise<any> {
        const isRtl = rtlDetect.isRtlLang(abp.localization.currentLanguage.name);

        if (isRtl) {
            document.documentElement.setAttribute('dir', 'rtl');
        }

        const cssPostfix = isRtl ? '-rtl' : '';
        const uiCustomizationService = new AppUiCustomizationService();
        const themeColor = uiCustomizationService.getThemeColor();
        const theme = uiCustomizationService.getTheme();
        const styleLoaderService = new StyleLoaderService();

        let styleUrls = [
            AppConsts.appBaseUrl + '/assets/metronic/assets/demo/' + themeColor + '/base/style.bundle' + cssPostfix.replace('-', '.') + '.css',
            AppConsts.appBaseUrl + '/assets/primeng/datatable/css/primeng.datatable' + cssPostfix + '.css',
            AppConsts.appBaseUrl + '/assets/common/styles/themes/' + themeColor + '/primeng.datatable' + cssPostfix + '.css',
            AppConsts.appBaseUrl + '/assets/common/styles/metronic-customize.css',
            AppConsts.appBaseUrl + '/assets/common/styles/themes/' + themeColor + '/metronic-customize.css',
            AppConsts.appBaseUrl + '/assets/common/styles/metronic-customize-angular.css',
            AppConsts.appBaseUrl + '/assets/common/styles/themes/' + themeColor + '/metronic-customize-angular.css',
            AppConsts.appBaseUrl + '/assets/themes/' + theme + '/' + theme + '-layout.component.css'
        ].concat(DynamicResourcesHelper.getAdditionalThemeAssets());

        styleLoaderService.loadArray(styleUrls);

        if (isRtl) {
            styleLoaderService.load(
                AppConsts.appBaseUrl + '/assets/common/styles/abp-zero-template-rtl.css'
            );
        }

        return Promise.resolve(true);
    }

    static getAdditionalThemeAssets(): string[] {
        const uiCustomizationService = new AppUiCustomizationService();
        const theme = uiCustomizationService.getTheme();

        if (theme === 'default') {
            return new DefaultThemeAssetContributor().getAssetUrls();
        }

        if (theme === 'demo8') {
            return new Demo8ThemeAssetContributor().getAssetUrls();
        }

        return [];
    }
}

import { BsDatepickerConfig, BsDaterangepickerConfig, BsLocaleService } from 'ngx-bootstrap/datepicker';
import { NgxBootstrapLocaleMappingService } from 'assets/ngx-bootstrap/ngx-bootstrap-locale-mapping.service';
import { defineLocale } from 'ngx-bootstrap/chronos';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';

export class NgxBootstrapDatePickerConfigService {

    static getDaterangepickerConfig(): BsDaterangepickerConfig {
        return Object.assign(new BsDaterangepickerConfig(), {
            containerClass: 'theme-' + NgxBootstrapDatePickerConfigService.getThemeColor()
        });
    }

    static getDatepickerConfig(): BsDatepickerConfig {
        return Object.assign(new BsDatepickerConfig(), {
            containerClass: 'theme-' + NgxBootstrapDatePickerConfigService.getThemeColor()
        });
    }

    static getThemeColor(): string {
        //todo: handle not existing colors for datepicker.
        return new AppUiCustomizationService().getThemeColor();
    }

    static getDatepickerLocale(): BsLocaleService {
        let localeService = new BsLocaleService();
        localeService.use(abp.localization.currentLanguage.name);
        return localeService;
    }

    static registerNgxBootstrapDatePickerLocales() {
        if (abp.localization.currentLanguage.name === 'en') {
            return;
        }

        let supportedLocale = new NgxBootstrapLocaleMappingService().map(abp.localization.currentLanguage.name).toLowerCase();
        let moduleLocaleName = new NgxBootstrapLocaleMappingService().getModuleName(abp.localization.currentLanguage.name);

        import(`ngx-bootstrap/chronos/i18n/${supportedLocale}.js`)
            .then(module => {
                defineLocale(abp.localization.currentLanguage.name.toLowerCase(), module[`${moduleLocaleName}Locale`]);
            });
    }
}

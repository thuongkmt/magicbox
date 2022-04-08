import { Component, Injector, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { UiCustomizationSettingsEditDto, UiCustomizationSettingsServiceProxy } from '@shared/service-proxies/service-proxies';

@Component({
    templateUrl: './demo8-theme-ui-settings.component.html',
    animations: [appModuleAnimation()],
    selector: 'demo8-theme-ui-settings'
})
export class Demo8ThemeUiSettingsComponent extends AppComponentBase implements OnInit {

    demo8Settings: UiCustomizationSettingsEditDto;

    constructor(
        injector: Injector,
        private _uiCustomizationService: UiCustomizationSettingsServiceProxy
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this._uiCustomizationService.getUiManagementSettings().subscribe((settingsResult) => {
            this.demo8Settings = settingsResult;
        });
    }

    getCustomizedSetting(settings: UiCustomizationSettingsEditDto) {
        settings.layout.theme = 'demo8';
        settings.layout.themeColor = 'default';
        settings.header.headerSkin = 'light';
        settings.menu.asideSkin = 'light';
        settings.menu.position = 'tab';

        return settings;
    }

    updateDefaultUiManagementSettings(): void {
        this._uiCustomizationService.updateDefaultUiManagementSettings(this.getCustomizedSetting(this.demo8Settings)).subscribe(() => {
            window.location.reload();
        });
    }

    updateUiManagementSettings(): void {
        this._uiCustomizationService.updateUiManagementSettings(this.getCustomizedSetting(this.demo8Settings)).subscribe(() => {
            window.location.reload();
        });
    }

    useSystemDefaultSettings(): void {
        this._uiCustomizationService.useSystemDefaultSettings().subscribe(() => {
            window.location.reload();
        });
    }
}

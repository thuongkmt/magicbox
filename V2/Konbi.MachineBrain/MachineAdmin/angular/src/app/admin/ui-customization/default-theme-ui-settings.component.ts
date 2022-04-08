import { Component, Injector, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { UiCustomizationSettingsEditDto, UiCustomizationSettingsServiceProxy } from '@shared/service-proxies/service-proxies';

@Component({
    templateUrl: './default-theme-ui-settings.component.html',
    animations: [appModuleAnimation()],
    selector: 'default-theme-ui-settings'
})
export class DefaultThemeUiSettingsComponent extends AppComponentBase implements OnInit {

    defaultSettings: UiCustomizationSettingsEditDto;

    constructor(
        injector: Injector,
        private _uiCustomizationService: UiCustomizationSettingsServiceProxy
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this._uiCustomizationService.getUiManagementSettings().subscribe((settingsResult) => {
            this.defaultSettings = settingsResult;
        });
    }

    getCustomizedSetting(settings: UiCustomizationSettingsEditDto) {
        settings.layout.theme = 'default';
        settings.menu.position = 'left';

        return settings;
    }

    updateDefaultUiManagementSettings(): void {
        this._uiCustomizationService.updateDefaultUiManagementSettings(this.getCustomizedSetting(this.defaultSettings)).subscribe(() => {
            window.location.reload();
        });
    }

    updateUiManagementSettings(): void {
        this._uiCustomizationService.updateUiManagementSettings(this.getCustomizedSetting(this.defaultSettings)).subscribe(() => {
            window.location.reload();
        });
    }

    useSystemDefaultSettings(): void {
        this._uiCustomizationService.useSystemDefaultSettings().subscribe(() => {
            window.location.reload();
        });
    }

    allowAsideMinimizingChange(val): void {
        if (val) {
            this.defaultSettings.menu.allowAsideHiding = false;
            this.defaultSettings.menu.defaultHiddenAside = false;
        } else {
            this.defaultSettings.menu.defaultMinimizedAside = false;
        }
    }

    allowAsideHidingChange(val): void {
        if (!val) {
            this.defaultSettings.menu.defaultHiddenAside = false;
        }
    }
}

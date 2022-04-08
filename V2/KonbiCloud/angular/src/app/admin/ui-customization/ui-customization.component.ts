import { Component, Injector, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
    templateUrl: './ui-customization.component.html',
    animations: [appModuleAnimation()]
})
export class UiCustomizationComponent extends AppComponentBase implements OnInit {

    theme: string;

    constructor(
        injector: Injector
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.theme = abp.setting.get('App.UiManagement.Theme').toLocaleLowerCase();
    }
}

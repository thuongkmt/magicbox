import { Injector, Component, ViewEncapsulation, Inject } from '@angular/core';

import { AppConsts } from '@shared/AppConsts';
import { AppComponentBase } from '@shared/common/app-component-base';

import { DOCUMENT } from '@angular/common';

@Component({
    templateUrl: './demo8-brand.component.html',
    selector: 'demo8-brand',
    encapsulation: ViewEncapsulation.None
})
export class Demo8BrandComponent extends AppComponentBase {

    defaultLogo = AppConsts.appBaseUrl + '/assets/common/images/app-logo-on-' + this.ui.getAsideSkin() + '.svg';
    remoteServiceBaseUrl: string = AppConsts.remoteServiceBaseUrl;

    constructor(
        injector: Injector,
        @Inject(DOCUMENT) private document: Document
    ) {
        super(injector);
    }

    clickTopbarToggle(): void {
        this.document.body.classList.toggle('m-topbar--on');
    }
}

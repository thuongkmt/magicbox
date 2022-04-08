import { Component, OnInit, Injector } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {AlertSettingAppServicesServiceProxy} from '@shared/service-proxies/alert-configuration-service-proxies';
import { finalize } from 'rxjs/operators';
import { AlertConfiguration } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: './alert-configurations.component.html',
  styleUrls: ['./alert-configurations.component.css'],
  animations: [appModuleAnimation()]
})
export class AlertConfigurationsComponent extends AppComponentBase {
    saving = false;
    alertConfiguration: AlertConfiguration = new AlertConfiguration();

    constructor(injector: Injector,
      private _alertSettingAppServicesServiceProxy: AlertSettingAppServicesServiceProxy) { 
      super(injector); 
    }

    ngOnInit() {
        this.getAlertConfigurations();
    }

    save(): void {
        this.saving = true;
        this._alertSettingAppServicesServiceProxy.updateAlertConfig(this.alertConfiguration)
            .pipe(finalize(() => { this.saving = false; }))
            .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
            });
    }

    getAlertConfigurations() {
        this._alertSettingAppServicesServiceProxy.getAlertConfiguration("").subscribe(result => {
            this.alertConfiguration = result;
        });
    }
}

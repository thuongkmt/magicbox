import { Component, OnInit, Injector } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {AlertSettingAppServicesServiceProxy,MachineStatus} from '@shared/service-proxies/alert-configuration-service-proxies';
import { finalize } from 'rxjs/operators';
import { AlertConfiguration } from '@shared/service-proxies/service-proxies';
import { SystemConfigServiceProxy, PagedResultDtoOfSystemConfigDto ,SystemConfigDto} from '@shared/service-proxies/system-config-service-proxies'; 

@Component({
  templateUrl: './alert-configurations.component.html',
  styleUrls: ['./alert-configurations.component.css'],
  animations: [appModuleAnimation()]
})

export class AlertConfigurationsComponent extends AppComponentBase {
    machineStatus = MachineStatus;
    saving = false;
    alertConfiguration: AlertConfiguration = new AlertConfiguration();
    normalTemperature = new SystemConfigDto({
        name:'RfidFridgeSetting.System.Temperature.NormalTemperature',
        value:""
    });

    stopSaleTimeSpan = new SystemConfigDto({
        name:'RfidFridgeSetting.Machine.StopSaleTimeSpan',
        value:""
    });

    constructor(injector: Injector,
      private _alertSettingAppServicesServiceProxy: AlertSettingAppServicesServiceProxy,
      private systemConfigService: SystemConfigServiceProxy) { 
      super(injector); 
    }

    ngOnInit() {
        this.getSetting();
        //this.getAlertConfigurations();
    }

    save(): void {
        this.saving = true;
        this.systemConfigService.update(this.normalTemperature)
            .pipe(finalize(() => { this.saving = false; }))
            .subscribe(() => {
              this.notify.info(this.l('Saved Successfully'));
            });
    }

    getAlertConfigurations() {
        this._alertSettingAppServicesServiceProxy.getAlertConfiguration("").subscribe(result => {
            this.alertConfiguration = result;
            console.log(this.alertConfiguration);
        });
    }

    getSetting() {
        this.systemConfigService.getAll()
            .subscribe((result: PagedResultDtoOfSystemConfigDto) => {
                this.normalTemperature = result.items.find(x => x.name == this.normalTemperature.name);
                this.stopSaleTimeSpan = result.items.find(x => x.name == this.stopSaleTimeSpan.name);
            });
    }

    changeMachineStatus(status: MachineStatus){
        this._alertSettingAppServicesServiceProxy.updateMachineStatus(status)
            .pipe(finalize(() => {  }))
            .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
            });
    }

    updateSettingsValue(value:SystemConfigDto){
        this.systemConfigService.update(value)
            .finally(() => { })
            .subscribe(() => {
              this.notify.info(this.l('Saved Successfully'));
            });
      }
}

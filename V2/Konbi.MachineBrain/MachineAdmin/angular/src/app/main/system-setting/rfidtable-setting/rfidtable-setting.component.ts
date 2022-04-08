import { Component, Injector, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { PagedListingComponentBase, PagedRequestDto } from 'shared/paged-listing-component-base';
import { SystemConfigServiceProxy, SystemConfigDto, PagedResultDtoOfSystemConfigDto } from '@shared/service-proxies/system-config-service-proxies';
import { EditSettingComponent } from 'app/main/system-setting/edit-setting/edit-setting.component';
import { RxStompService } from '@stomp/ng2-stompjs';

@Component({
  selector: 'app-rfidtable-setting',
  templateUrl: './rfidtable-setting.component.html',
  styleUrls: ['./rfidtable-setting.component.css']
})
export class RfidtableSettingComponent extends AppComponentBase {

  private editField: string;
  public searchText: string;

  allSetting: Array<any> = [];
  settingList: Array<any> = [];

  @ViewChild('editSettingModal') editSettingModal: EditSettingComponent;

  constructor(
    private injector: Injector,
    private rxStompService: RxStompService,
    private systemConfigService: SystemConfigServiceProxy) {
    super(injector);

    this.getConfigs();
  }


  updateList(id: number, config: any, event: any) {
    const editField = event.target.textContent;
    if (config.value !== editField) {
      this.settingList[id]['value'] = editField;
      //call update service
      let updateConfig = new SystemConfigDto();
      updateConfig.name = config.name;
      updateConfig.value = editField;

      this.systemConfigService.update(updateConfig)
        .finally(() => { })
        .subscribe(() => {
          this.refreshSettings();
          this.notify.info(this.l('Saved Successfully'));
        });
    }
  }

  refreshSettings() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'MACHINE_REFRESHSETTINGS' });
  }

  searchTextChange(newValue) {
    this.settingList = this.allSetting.filter(function (item) {
      return item.name.toLowerCase().indexOf(newValue.toLowerCase()) !== -1;
    });
  }

  changeValue(id: number, config: any, event: any) {
    this.editField = event.target.textContent;
    //console.log(this.editField);
  }

  getConfigs(): void {
    this.systemConfigService.getAllByName('RfidFridgeSetting')
      .subscribe((result: PagedResultDtoOfSystemConfigDto) => {
        this.allSetting = result.items;
        this.settingList = result.items;
      });
  }

  edit(name) {
    this.editSettingModal.show();
    console.log(name);
  }

  delete(systemConfigDto: SystemConfigDto): void {
  }

}

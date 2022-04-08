import { Component, Injector, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/app-component-base';
import { PagedListingComponentBase, PagedRequestDto } from "shared/paged-listing-component-base";
import { SystemConfigServiceProxy, SystemConfigDto, PagedResultDtoOfSystemConfigDto } from '@shared/service-proxies/system-config-service-proxies';


@Component({
  selector: 'app-abp-setting',
  templateUrl: './abp-setting.component.html',
  styleUrls: ['./abp-setting.component.css'],
  animations: [appModuleAnimation()]
})
export class AbpSettingComponent extends AppComponentBase {

  private editField: string;
  public searchText: string;

  allSetting: Array<any> = [];
  settingList: Array<any> = [];


  constructor(
    private injector: Injector,
    private systemConfigService: SystemConfigServiceProxy) {
    super(injector);

    this.getConfigs();
  }


  updateList(id: number, config: any, event: any) {
    const editField = event.target.textContent;
    if (config.value != editField) {
      this.settingList[id]['value'] = editField;
      //call update service
      var updateConfig = new SystemConfigDto();
      updateConfig.name = config.name;
      updateConfig.value = editField;
      
      this.systemConfigService.update(updateConfig)
        .finally(() => { })
        .subscribe(() => {
          this.notify.info(this.l('Saved Successfully'));
        });
    }
  }

  searchTextChange(newValue) {
    this.settingList = this.allSetting.filter(function (item) {
      return item.name.toLowerCase().indexOf(newValue.toLowerCase()) !== -1;
    })
  }

  changeValue(id: number, config: any, event: any) {
    this.editField = event.target.textContent;
    //console.log(this.editField);
  }


  getConfigs(): void {
console.log("call here");
    this.systemConfigService.getAll()
      .subscribe((result: PagedResultDtoOfSystemConfigDto) => {
        this.allSetting = result.items
        this.settingList = result.items
      });
  }

  delete(systemConfigDto: SystemConfigDto): void {
  }

}

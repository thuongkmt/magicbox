import { Component, OnInit, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Http } from '@angular/http';
import { PlateServiceProxy  } from '@shared/service-proxies/plate-service-proxies';
import { SessionServiceProxy  } from '@shared/service-proxies/service-proxies';
import { PlateMenuServiceProxy } from '@shared/service-proxies/plate-menu-service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import * as _ from 'lodash';
import { TrayServiceProxy, TrayDto } from '@shared/service-proxies/tray-service-proxies';
// import { DiscServiceProxy, DiscDto } from '@shared/service-proxies/service-proxies';
import { PlateCategoryServiceProxy, PlateCategoryDto } from '@shared/service-proxies/plate-category-service-proxies';

@Component({
  selector: 'app-sync-setting',
  templateUrl: './sync-setting.component.html',
  styleUrls: ['./sync-setting.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})
export class SyncSettingComponent extends AppComponentBase {

  constructor(
    injector: Injector,
    private _plateMenusServiceProxy: PlateMenuServiceProxy,
    private _sessionsServiceProxy: SessionServiceProxy,
    private _trayService: TrayServiceProxy,
    private _platesServiceProxy: PlateServiceProxy,
    //private _discsServiceProxy: DiscsServiceProxy,
    private _plateCategoriesServiceProxy: PlateCategoryServiceProxy,
  ) {
      super(injector);
  }

  ngOnInit() {
  }
  syncSession()
  {
    this.primengTableHelper.showLoadingIndicator();
        // t//his._sessionsServiceProxy.syncSessionData()
        // .finally(() => {
        //     this.primengTableHelper.hideLoadingIndicator();
        // })
        // .subscribe(() => {
        //     this.primengTableHelper.hideLoadingIndicator();
        //     this.notify.success('Sessions synced');
        // });
  }
  syncTray()
  {
    this.primengTableHelper.showLoadingIndicator();
        this._trayService.syncTray()
        .finally(() => {
            this.primengTableHelper.hideLoadingIndicator();
        })
        .subscribe(() => {
            this.primengTableHelper.hideLoadingIndicator();
            this.notify.success('Trays synced');
        });
  }
  syncPlate()
  {
    this.primengTableHelper.showLoadingIndicator();
    this._platesServiceProxy.syncPlate()
        .finally(() => {
            this.primengTableHelper.hideLoadingIndicator();
        })
        .subscribe(() => {
            this.primengTableHelper.hideLoadingIndicator();
            this.notify.success('Plate synced');
        });
  }
  syncPlateCategory()
  {
    this.primengTableHelper.showLoadingIndicator();
    this._plateCategoriesServiceProxy.syncData()
        .finally(() => {
            this.primengTableHelper.hideLoadingIndicator();
        })
        .subscribe(() => {
            this.primengTableHelper.hideLoadingIndicator();
            this.notify.success('Plate Category synced');
        });
  }
  syncInventory()
  {
    this.primengTableHelper.showLoadingIndicator();
    // this._discsServiceProxy.syncPlateDataFromServer()
    //     .finally(() => {
    //         this.primengTableHelper.hideLoadingIndicator();
    //     })
    //     .subscribe(() => {
    //         this.primengTableHelper.hideLoadingIndicator();
    //         this.notify.success('Inventory synced');
    //     });
  }
  syncMenuScheduler()
  {
    this.primengTableHelper.showLoadingIndicator();
    this._plateMenusServiceProxy.syncData()
        .finally(() => {
            this.primengTableHelper.hideLoadingIndicator();
        })
        .subscribe(() => {
            this.primengTableHelper.hideLoadingIndicator();
            this.notify.success('Plate Menus synced');
        });
  }
}

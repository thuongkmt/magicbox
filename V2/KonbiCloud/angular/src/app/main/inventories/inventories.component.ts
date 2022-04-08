import { Component, Injector, ViewEncapsulation, ViewChild, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { InventoriesServiceProxy, InventoryOverviewDto } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import * as _ from 'lodash';
import * as moment from 'moment';

@Component({
    templateUrl: './inventories.component.html',
    styleUrls: ['./inventories.component.less'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class InventoriesComponent extends AppComponentBase implements OnInit {
    inventories:  InventoryOverviewDto[] = [];

    constructor(
        injector: Injector,
        private _inventoryServiceProxy: InventoriesServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _router: Router,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.getInventories();
    }

    goToDetail(item) {
        const url = `app/main/inventories/inventoryDetail/${item.machineId}/${item.topupId}`;
        this._router.navigate([url]);
    }

    getInventories() {
        this.primengTableHelper.showLoadingIndicator();
        this._inventoryServiceProxy.getInventoryOverview().subscribe(result => {
            this.inventories = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }
}

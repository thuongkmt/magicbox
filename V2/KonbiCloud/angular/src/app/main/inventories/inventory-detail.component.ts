import { Component, Injector, ViewEncapsulation, ViewChild, OnInit } from '@angular/core';
import { formatDate } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { Http } from '@angular/http';
import { InventoriesServiceProxy, InventoryDetailOutput } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import * as _ from 'lodash';
import * as moment from 'moment';

@Component({
    templateUrl: './inventory-detail.component.html',
    styleUrls: ['./inventory-detail.component.less'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class InventoryDetailComponent extends AppComponentBase implements OnInit {
    inventoryDetail: InventoryDetailOutput = new InventoryDetailOutput();

    constructor(
        injector: Injector,
        private _inventoryServiceProxy: InventoriesServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _route: ActivatedRoute,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.loadInventoryDetail();
    }

    loadInventoryDetail () {
        this.primengTableHelper.showLoadingIndicator();

        const machineId = this._route.snapshot.paramMap.get("machineId");
        const topupId = this._route.snapshot.paramMap.get("topupId");

        this._inventoryServiceProxy.getInventoryDetail(machineId, topupId).subscribe(result => {
            this.inventoryDetail = result;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }
}

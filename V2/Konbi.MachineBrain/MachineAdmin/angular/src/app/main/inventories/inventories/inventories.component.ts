import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { InventoriesServiceProxy, InventoryDto } from '@shared/service-proxies/inventories-service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { CreateOrEditInventoryModalComponent } from './create-or-edit-inventory-modal.component';
import { ViewInventoryModalComponent } from './view-inventory-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';
import { RxStompService } from '@stomp/ng2-stompjs';
import { HttpClient } from '@angular/common/http';
import { AppConsts } from '@shared/AppConsts';


@Component({
    templateUrl: './inventories.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class InventoriesComponent extends AppComponentBase {

    @ViewChild('createOrEditInventoryModal') createOrEditInventoryModal: CreateOrEditInventoryModalComponent;
    @ViewChild('viewInventoryModalComponent') viewInventoryModal: ViewInventoryModalComponent;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;

    advancedFiltersAreShown = false;
    filterText = '';
    tagIdFilter = '';
    maxTrayLevelFilter: number;
    maxTrayLevelFilterEmpty: number;
    minTrayLevelFilter: number;
    minTrayLevelFilterEmpty: number;
    maxPriceFilter: number;
    maxPriceFilterEmpty: number;
    minPriceFilter: number;
    minPriceFilterEmpty: number;
    productNameFilter = '';
    private _baseUrl: string;


    constructor(
        injector: Injector,
        private _inventoriesServiceProxy: InventoriesServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _activatedRoute: ActivatedRoute,
        private _fileDownloadService: FileDownloadService,
        private rxStompService: RxStompService,
        private _httpClient: HttpClient
    ) {
        super(injector);
        this._baseUrl = AppConsts.remoteServiceBaseUrl;
    }

    getInventories(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();

        this._inventoriesServiceProxy.getAll(
            this.filterText,
            this.tagIdFilter,
            this.maxTrayLevelFilter == null ? this.maxTrayLevelFilterEmpty : this.maxTrayLevelFilter,
            this.minTrayLevelFilter == null ? this.minTrayLevelFilterEmpty : this.minTrayLevelFilter,
            this.maxPriceFilter == null ? this.maxPriceFilterEmpty : this.maxPriceFilter,
            this.minPriceFilter == null ? this.minPriceFilterEmpty : this.minPriceFilter,
            this.productNameFilter,
            this.primengTableHelper.getSorting(this.dataTable),
            this.primengTableHelper.getSkipCount(this.paginator, event),
            this.primengTableHelper.getMaxResultCount(this.paginator, event)
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    reloadPage(): void {
        this.paginator.changePage(this.paginator.getPage());
    }

    createInventory(): void {
        this.createOrEditInventoryModal.show();
    }

    // deleteInventory(inventory: InventoryDto): void {
    //     this.message.confirm(
    //         '',
    //         (isConfirmed) => {
    //             if (isConfirmed) {
    //                 this._inventoriesServiceProxy.delete(inventory.id)
    //                     .subscribe(() => {
    //                         this.reloadPage();
    //                         this.RefreshProduct();
    //                         this.notify.success(this.l('Successfully Deleted'));
    //                     });
    //             }
    //         }
    //     );
    // }

    RefreshProduct() {
        this.rxStompService.publish({ destination: '/topic/command', body: 'MACHINE_REFRESHPRD' });
    }

    exportToExcel(): void {
        this._inventoriesServiceProxy.getInventoriesToExcel(
            this.filterText,
            this.tagIdFilter,
            this.maxTrayLevelFilter == null ? this.maxTrayLevelFilterEmpty : this.maxTrayLevelFilter,
            this.minTrayLevelFilter == null ? this.minTrayLevelFilterEmpty : this.minTrayLevelFilter,
            this.maxPriceFilter == null ? this.maxPriceFilterEmpty : this.maxPriceFilter,
            this.minPriceFilter == null ? this.minPriceFilterEmpty : this.minPriceFilter,
            this.productNameFilter,
        )
            .subscribe(result => {
                this._fileDownloadService.downloadTempFile(result);
            });
    }
}

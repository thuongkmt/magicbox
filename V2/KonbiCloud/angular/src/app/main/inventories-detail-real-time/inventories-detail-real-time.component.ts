import { Component, Injector, ViewEncapsulation, ViewChild, OnInit, NgZone } from '@angular/core';
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
import { MagicBoxSignalrService } from '@app/shared/common/signalr/magicbox-signalr.service';
import { SignalRHelper } from '@shared/helpers/SignalRHelper';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import * as $ from 'jquery';

@Component({
    templateUrl: './inventories-detail-real-time.component.html',
    styleUrls: ['./inventories-detail-real-time.component.less'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class InventoriesDetailRealTimeComponent extends AppComponentBase implements OnInit {

    inventoryDetail: InventoryDetailOutput = new InventoryDetailOutput();
    eventsRegistered: boolean = false;
    differCount: number = 0;
    reloadDataTaskId: any;
    constructor(
        injector: Injector,
        private _inventoryServiceProxy: InventoriesServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _route: ActivatedRoute,
        public _zone: NgZone,
        private _magicBoxSignalrService: MagicBoxSignalrService,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.loadInventoryDetail();
        if (this.appSession.application) {
            SignalRHelper.initSignalR(() => {
                this._magicBoxSignalrService.init();
            });
        }
        this.registerEvents();
    }

    loadInventoryDetail() {
        this.primengTableHelper.showLoadingIndicator();

        const machineId = this._route.snapshot.paramMap.get("machineId");
        const topupId = this._route.snapshot.paramMap.get("topupId");

        this._inventoryServiceProxy.getMachineInventoryDetailRealTime(machineId, topupId).subscribe(result => {
            this.inventoryDetail = result;
            this.primengTableHelper.totalRecordsCount = result.inventoryDetailList.length;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    exportToCsv(): void {
        // Check exists data export.
        //if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
        // Show icon loading.
        this.primengTableHelper.showLoadingIndicator();
        // Load transactions.
        const machineId = this._route.snapshot.paramMap.get("machineId");
        this._inventoryServiceProxy.getInventoryForReportByMachine(machineId).subscribe(result => {
            if (result != null) {
                // Declare csv data.
                let csvData = new Array();
                // Declare const header.
                const header = {
                    MachineId: 'Machine ID',
                    ProductName: 'Product Name',
                    ProductCategory: 'Product Category',
                    SKU: 'Product SKU',
                    ProductDescription: 'Product Description',
                    ProductPrice: 'Product Price',
                    Quantity: 'Quantity',
                    Sold: 'Sold',
                    Missing: 'Missing'
                };
                // Add header to csv data.
                csvData.push(header);
                // add content file csv.
                for (let record of result) {
                    for (let data of record.inventoryDetailList) {
                        csvData.push({
                            MachineId: (record.machineName == null) ? '' : record.machineName,
                            ProductName: (data.productName == null) ? '' : data.productName,
                            ProductCategory: (data.productCategory == null) ? '' : data.productCategory,
                            SKU: (data.productSku == null) ? '' : data.productSku,
                            ProductDescription: (data.productDescription == null) ? '' : data.productDescription,
                            ProductPrice: (data.productPrice == null) ? '' : data.productPrice,
                            Quantity: (data.total == null) ? '' : data.total,
                            Sold: (data.sold == null) ? '' : data.sold,
                            Missing: (data.missing == null) ? '' : data.missing,
                        });
                    }

                }
                // Export csv data.
                // tslint:disable-next-line:no-unused-expression
                new Angular5Csv(csvData, 'Inventory_' + this.inventoryDetail.machineName, { quoteStrings: '' });
                // tslint:enable-next-line:no-unused-expression
            } else {
                this.notify.info(this.l('Data is Empty'));
            }
            this.primengTableHelper.hideLoadingIndicator();
        });
        // } else {
        //     this.notify.info(this.l('Data is Empty'));
        // }
    }
    requestUpdateStock(event: any): void {
        let machineId = this._route.snapshot.paramMap.get("machineId");
        console.log(event);
        console.log(machineId);
        let target = $(event.target)
        // change icon to spinner
        target.removeClass('fa-retweet').addClass('fa-spin fa-spinner');
        this._inventoryServiceProxy.requestMachineToUpdateInventory(machineId)

            .finally(() => {
                // change icon back to normal after 3s
                if (target.hasClass('fa-spin')) {
                    target.addClass('fa-retweet').removeClass('fa-spin fa-spinner');
                }
            })
            .subscribe(result => {
                console.log(result);
            });
        console.log('ok');


    }
    registerEvents(): void {
        if (this.eventsRegistered)
            return;
        const self = this;
        this.eventsRegistered = true;


        function onMessageReceived(message: any) {
            console.log("DifferCount: ", self.differCount);
            if (self.reloadDataTaskId) {
                clearTimeout(self.reloadDataTaskId);
                self.differCount++;
            }
            const machineId = self._route.snapshot.paramMap.get("machineId");
            const topupId = self._route.snapshot.paramMap.get("topupId");
            if (self.differCount > 10) {

                self._inventoryServiceProxy.getMachineInventoryDetailRealTime(machineId, topupId)
                    .finally(() => { self.primengTableHelper.hideLoadingIndicator(); })
                    .subscribe(result => {
                        self.inventoryDetail = result;
                        self.primengTableHelper.totalRecordsCount = result.inventoryDetailList.length;
                        self.primengTableHelper.hideLoadingIndicator();
                    });
                self.differCount = 0;
            } else {
                //register a task to run after xx seconds if no new updates 
                self.reloadDataTaskId = setTimeout(() => {
                    self._inventoryServiceProxy.getMachineInventoryDetailRealTime(machineId, topupId)
                        .finally(() => { self.primengTableHelper.hideLoadingIndicator(); })
                        .subscribe(result => {
                            self.inventoryDetail = result;
                            self.primengTableHelper.totalRecordsCount = result.inventoryDetailList.length;
                            self.primengTableHelper.hideLoadingIndicator();
                        });
                    self.differCount = 0;
                }, 500);
            }


        };

        abp.event.on('Topup', message => {
            self._zone.run(() => {
                onMessageReceived(message);
            });
        });

        abp.event.on('Transaction', message => {
            self._zone.run(() => {
                onMessageReceived(message);
            });
        });

        abp.event.on('CurrentInventory', message => {
            self._zone.run(() => {
                onMessageReceived(message);
            });
        });
    }

}

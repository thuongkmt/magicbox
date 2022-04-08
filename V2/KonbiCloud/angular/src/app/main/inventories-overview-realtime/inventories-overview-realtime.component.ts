import { Component, Injector, ViewEncapsulation, ViewChild, OnInit, OnDestroy, NgZone } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { InventoriesServiceProxy, InventoryOverviewDto } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import * as _ from 'lodash';
import * as moment from 'moment';
import * as $ from 'jquery';
import { SignalRHelper } from '@shared/helpers/SignalRHelper';
import { MagicBoxSignalrService } from '@app/shared/common/signalr/magicbox-signalr.service';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';

@Component({
    templateUrl: './inventories-overview-realtime.component.html',
    styleUrls: ['./inventories-overview-realtime.component.less'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class InventoriesOverviewRealtimeComponent extends AppComponentBase implements OnInit {

    inventories: InventoryOverviewDto[] = [];
    differCount: number = 0;
    reloadDataTaskId: any;
    eventsRegistered: Boolean = false;

    constructor(
        injector: Injector,
        private _inventoryServiceProxy: InventoriesServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _router: Router,
        public _zone: NgZone,
        private _magicBoxSignalrService: MagicBoxSignalrService,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        console.log('ngOnInit');
        this.getInventories();
        if (this.appSession.application) {
            SignalRHelper.initSignalR(() => {
                this._magicBoxSignalrService.init();
            });
        }
        this.registerEvents();
    }
    OnDestroy(): void {
        console.log('ngOnDestroy');
    }

    goToDetail(item) {
        const url = `app/main/inventories-detail-real-time/inventories-detail-real-time/${item.machineId}/${item.topupId}`;
        this._router.navigate([url]);
    }



    // Export list transactions to csv.exportTnxItemsCvs
    exportToCsv(): void {
        // Check exists data export.
        //if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
        // Show icon loading.
        this.primengTableHelper.showLoadingIndicator();
        // Load transactions.
        this._inventoryServiceProxy.getInventoryForReport().subscribe(result => {
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
                new Angular5Csv(csvData, 'Inventory', { quoteStrings: '' });
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
    canRefresh(lastUpdated?: moment.Moment): Boolean {
        if (!lastUpdated)
            return true;
        return moment().diff(lastUpdated) > 60000; // 1 minute
    }
    // requestUpdateStock(event: any, machineId: string): void {
    //     console.log(event);
    //     console.log(machineId);
    //     let target = $(event.target)
    //     // change icon to spinner
    //     target.removeClass('fa-retweet').addClass('fa-spin fa-spinner');
    //     this._inventoryServiceProxy.requestMachineToUpdateInventory(machineId)
    //         .finally(() => {
    //             // change icon back to normal after 3s
    //             if (target.hasClass('fa-spin')) {
    //                 target.addClass('fa-retweet').removeClass('fa-spin fa-spinner');
    //             }
    //         });
    //     console.log('ok');


    // }

    getInventories() {
        this.primengTableHelper.showLoadingIndicator();
        this._inventoryServiceProxy.getMachinesInventoryRealTime().subscribe(result => {
            this.inventories = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
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
            if (self.differCount > 10) {
                self._inventoryServiceProxy.getMachinesInventoryRealTime()
                    .finally(() => { self.primengTableHelper.hideLoadingIndicator(); })
                    .subscribe(result => {
                        self.inventories = result.items;
                        self.primengTableHelper.hideLoadingIndicator();
                    });
                self.differCount = 0;
            } else {
                //register a task to run after xx seconds if no new updates 
                self.reloadDataTaskId = setTimeout(() => {
                    self._inventoryServiceProxy.getMachinesInventoryRealTime()
                        .finally(() => { self.primengTableHelper.hideLoadingIndicator(); })
                        .subscribe(result => {
                            self.inventories = result.items;
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

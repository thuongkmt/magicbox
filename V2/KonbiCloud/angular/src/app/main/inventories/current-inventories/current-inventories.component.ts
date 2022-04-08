import { Component, Injector, ViewEncapsulation, ViewChild, OnInit, NgZone } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { InventoriesServiceProxy, GetCurrentInventoryInput } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import * as _ from 'lodash';
import * as moment from 'moment';
import { SignalRHelper } from 'shared/helpers/SignalRHelper';
import { MagicBoxSignalrService } from '../../../shared/common/signalr/magicbox-signalr.service';
import { MachineServiceProxy } from "shared/service-proxies/machine-service-proxies";

@Component({
    templateUrl: './current-inventories.component.html',
    styleUrls: ['./current-inventories.component.less'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class CurrentInventoriesComponent extends AppComponentBase implements OnInit {
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;
    filter = '';
    tagIdFilter = '';
    dropdownList = [];
    selectedItems = [];
    dropdownSettings = {};
    loadEvent: LazyLoadEvent;
    isLoading = true;
    eventsRegistered: boolean = false;
    differCount: number = 0;
    reloadDataTaskId: any;

    constructor(
        injector: Injector,
        private _inventoryServiceProxy: InventoriesServiceProxy,
        private _magicBoxSignalrService: MagicBoxSignalrService,
        public _zone: NgZone,
        private _machinesService: MachineServiceProxy
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.getMachines();
        if (this.appSession.application) {
            SignalRHelper.initSignalR(() => {
                this._magicBoxSignalrService.init();
            });
        }
        this.registerEvents();
        this.dropdownSettings = {
            singleSelection: false,
            idField: 'item_id',
            textField: 'item_text',
            selectAllText: 'Select All',
            unSelectAllText: 'UnSelect All',
            itemsShowLimit: 3,
            allowSearchFilter: true
        };
    }

    getInventories(event?: LazyLoadEvent) {
        this.loadEvent = event;
        if (this.isLoading == true) {
            return;
        }
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }
        var machinesFilter = this.selectedItems.map(a => a.item_id);

        this.primengTableHelper.showLoadingIndicator();
        var input = new GetCurrentInventoryInput();
        input.filter = this.filter;
        input.tagIdFilter = this.tagIdFilter;
        input.machinesFilter = machinesFilter;
        input.sorting = this.primengTableHelper.getSorting(this.dataTable);
        input.maxResultCount = this.primengTableHelper.getMaxResultCount(this.paginator, event);
        input.skipCount = this.primengTableHelper.getSkipCount(this.paginator, event);
        this._inventoryServiceProxy.currentInventories(
            input
        )
            .finally(() => { this.primengTableHelper.hideLoadingIndicator(); })
            .subscribe(result => {
                this.primengTableHelper.totalRecordsCount = result.totalCount;
                this.primengTableHelper.records = result.items;
                this.primengTableHelper.hideLoadingIndicator();
            });
    }

    // Get all machines.
    getMachines(): void {
        this.primengTableHelper.showLoadingIndicator();
        this._machinesService.getAll(0, 999, 'name asc')
            .finally(() => { this.primengTableHelper.hideLoadingIndicator(); })
            .subscribe((result) => {
                let machines = [];
                for (let entry of result.items) {
                    let ChartLineItem = {
                        'item_id': entry.id,
                        'item_text': entry.name
                    };
                    machines.push(ChartLineItem);
                }
                this.dropdownList = machines;
                this.selectedItems = machines;
                //this.primengTableHelper.hideLoadingIndicator();
                this.isLoading = false;
                this.getInventories(this.loadEvent);
            });
    }

    registerEvents(): void {
        if (this.eventsRegistered)
            return;
        const self = this;
        self.eventsRegistered = true;
        function onMessageReceived(message: any) {
            console.log("DifferCount: ", self.differCount);
            if (self.reloadDataTaskId) {
                clearTimeout(self.reloadDataTaskId);
                self.differCount++;
            }

            if (self.differCount > 10) {

                if (self.primengTableHelper.shouldResetPaging(self.loadEvent)) {
                    self.paginator.changePage(0);
                    return;
                }
                var machinesFilter = self.selectedItems.map(a => a.item_id);
                self.primengTableHelper.showLoadingIndicator();
                var input = new GetCurrentInventoryInput();
                input.filter = self.filter;
                input.tagIdFilter = self.tagIdFilter;
                input.machinesFilter = machinesFilter;
                input.sorting = self.primengTableHelper.getSorting(self.dataTable);
                input.maxResultCount = self.primengTableHelper.getMaxResultCount(self.paginator, self.loadEvent);
                input.skipCount = self.primengTableHelper.getSkipCount(self.paginator, self.loadEvent);
                self._inventoryServiceProxy.currentInventories(
                    input
                )
                    .finally(() => { self.primengTableHelper.hideLoadingIndicator(); })
                    .subscribe(result => {
                        self.primengTableHelper.totalRecordsCount = result.totalCount;
                        self.primengTableHelper.records = result.items;
                        self.primengTableHelper.hideLoadingIndicator();
                    });
                self.differCount = 0;
            } else {
                //register a task to run after xx seconds if no new updates 
                self.reloadDataTaskId = setTimeout(() => {
                    if (self.primengTableHelper.shouldResetPaging(self.loadEvent)) {
                        self.paginator.changePage(0);
                        return;
                    }
                    var machinesFilter = self.selectedItems.map(a => a.item_id);
                    self.primengTableHelper.showLoadingIndicator();
                    var input = new GetCurrentInventoryInput();
                    input.filter = self.filter;
                    input.tagIdFilter = self.tagIdFilter;
                    input.machinesFilter = machinesFilter;
                    input.sorting = self.primengTableHelper.getSorting(self.dataTable);
                    input.maxResultCount = self.primengTableHelper.getMaxResultCount(self.paginator, self.loadEvent);
                    input.skipCount = self.primengTableHelper.getSkipCount(self.paginator, self.loadEvent);
                    self._inventoryServiceProxy.currentInventories(
                        input
                    )
                        .finally(() => { self.primengTableHelper.hideLoadingIndicator(); })
                        .subscribe(result => {
                            self.primengTableHelper.totalRecordsCount = result.totalCount;
                            self.primengTableHelper.records = result.items;
                            self.primengTableHelper.hideLoadingIndicator();
                        });
                    self.differCount = 0;
                }, 500);
            }


        };


        abp.event.on('CurrentInventory', message => {
            self._zone.run(() => {
                onMessageReceived(message);
            });
        });
    }

    // Event select single machine.
    onItemSelect(item: any) {
        if (this.selectedItems.length > 0) {
            this.getInventories();
        }
    }

    onDeSelect(item: any) {
        this.getInventories();
    }

    // Event select all machine.
    onSelectAll(items: any) {
        this.selectedItems = items;
        this.getInventories();
    }

    // Event unselect all machine.
    onDeSelectAll(items: any) {
        this.selectedItems = [];
        this.getInventories();
    }
}

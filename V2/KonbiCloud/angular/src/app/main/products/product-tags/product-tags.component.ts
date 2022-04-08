import { Component, OnInit, ViewChild, Injector, NgZone } from '@angular/core';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { ProductTagsServiceProxy, ProductTagDto } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { SignalRHelper } from 'shared/helpers/SignalRHelper';
import { MagicBoxSignalrService } from '../../../shared/common/signalr/magicbox-signalr.service';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import { MapTagByRanageComponent } from './map-tag-by-ranage/map-tag-by-ranage.component';
import * as moment from 'moment';
@Component({
    templateUrl: './product-tags.component.html',
    styleUrls: ['./product-tags.component.css'],
    animations: [appModuleAnimation()]
})
export class ProductTagsComponent extends AppComponentBase {

    @ViewChild('mapTagByRange') mapTagByRange: MapTagByRanageComponent;

    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;
    tagFilter = '';
    productFilter = '';
    stateFilter = undefined;
    // fromDateObject: Date | null;
    // toDateObject: Date | null;
    fromDateFilter: string;
    toDateFilter: string;
    loadEvent: LazyLoadEvent;
    stateEnum = [
        { key: '-- All --', value: null },
        { key: 'Mapped', value: 0 },
        { key: 'Stocked', value: 1 },
        { key: 'Sold', value: 2 }
    ];

    stateValue = { 0: 'Mapped', 1: 'Stocked', 2: 'Sold' };

    subscriptionEndDateUtcIsValid = false;
    constructor(
        injector: Injector,
        private _productTagServiceProxy: ProductTagsServiceProxy,
        private _magicBoxSignalrService: MagicBoxSignalrService,
        private _notifyService: NotifyService,
        public _zone: NgZone,
    ) {
        super(injector);
    }

    mapTagByRangeClick() {
        this.mapTagByRange.show();
    }

    getProductTags(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }
        this.primengTableHelper.showLoadingIndicator();

        this._productTagServiceProxy.getAll(
            this.tagFilter,
            this.productFilter,
            this.stateFilter,
            this.fromDateFilter ? moment(this.fromDateFilter) : undefined,
            this.toDateFilter ? moment(this.toDateFilter) : undefined,
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

    deleteProductTag(productTag: ProductTagDto): void {
        this.message.confirm(
            '',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._productTagServiceProxy.delete(productTag.id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }

    registerEvents(): void {
        const self = this;
        function onMessageReceived(message: any) {
            if (self.primengTableHelper.shouldResetPaging(self.loadEvent)) {
                self.paginator.changePage(0);
                return;
            }
            self.primengTableHelper.showLoadingIndicator();
            let sorting = self.primengTableHelper.getSorting(self.dataTable);
            let maxResultCount = self.primengTableHelper.getMaxResultCount(self.paginator, self.loadEvent);
            let skipCount = self.primengTableHelper.getSkipCount(self.paginator, self.loadEvent);
            self._productTagServiceProxy.getAll(
                self.tagFilter,
                self.productFilter,
                self.stateFilter,
                this.fromDateFilter ? moment(this.fromDateFilter) : undefined,
                this.toDateFilter ? moment(this.toDateFilter) : undefined,
                sorting,
                skipCount,
                maxResultCount
            )
                .finally(() => { self.primengTableHelper.hideLoadingIndicator(); })
                .subscribe(result => {
                    self.primengTableHelper.totalRecordsCount = result.totalCount;
                    self.primengTableHelper.records = result.items;
                    self.primengTableHelper.hideLoadingIndicator();
                });
        };

        abp.event.on('ProductTag', message => {
            self._zone.run(() => {
                onMessageReceived(message);
            });
        });
    }

    ngOnInit() {
        if (this.appSession.application) {
            SignalRHelper.initSignalR(() => {
                this._magicBoxSignalrService.init();
            });
        }
        this.registerEvents();
    }



    exportToCsv(): void {
        // Check exists data export.
        if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
            // Show icon loading.
            this.primengTableHelper.showLoadingIndicator();
            // Load transactions.
            this._productTagServiceProxy.getAllForReport(
                this.tagFilter,
                this.productFilter,
                this.stateFilter,
                this.fromDateFilter ? moment(this.fromDateFilter) : undefined,
                this.toDateFilter ? moment(this.toDateFilter) : undefined,
                undefined,
                0,
                1000,

            ).subscribe(result => {
                if (result.items != null) {
                    // Declare csv data.
                    let csvData = new Array();
                    // Declare const header.
                    const header = {
                        ProductName: 'Product Name',
                        ProductCategory: 'Product Category',
                        SKU: 'Product SKU',
                        TagId: 'RFID Tag',
                        State: 'State',
                        RestockedDate: 'Restocked Date'
                    };
                    // Add header to csv data.
                    csvData.push(header);
                    // add content file csv.
                    for (let record of result.items) {
                        csvData.push({
                            ProductName: (record.productName == null) ? '' : record.productName,
                            ProductCategory: (record.category == null) ? '' : record.category,
                            SKU: (record.sku == null) ? '' : record.sku,
                            TagId: (record.tagId == null) ? '' : record.tagId,
                            State: (record.state == null) ? '' : record.state,
                            RestockedDate: (record.creationTime == null) ? '' : moment(record.creationTime).format('DD/MM/YYYY HH:mm')
                        });
                    }
                    // Export csv data.
                    // tslint:disable-next-line:no-unused-expression
                    new Angular5Csv(csvData, 'MagicboxProductRFIDReport', { quoteStrings: '' });
                    // tslint:enable-next-line:no-unused-expression
                } else {
                    this.notify.info(this.l('Data is Empty'));
                }
                this.primengTableHelper.hideLoadingIndicator();
            });
        } else {
            this.notify.info(this.l('Data is Empty'));
        }
    }

}

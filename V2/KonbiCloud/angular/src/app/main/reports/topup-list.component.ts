import { Component, OnInit, Injector, ViewEncapsulation, ViewChild, Inject, Optional, } from '@angular/core';
import { Router } from '@angular/router';
import { TopupReportServiceProxy, TopupListDto } from '@shared/service-proxies/service-proxies';
import { TopupDetailComponent } from './topup-detail.component';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import * as _ from 'lodash';
import * as moment from 'moment';
import { MachineServiceProxy, MachineDto, PagedResultDtoOfMachineDto } from "shared/service-proxies/machine-service-proxies";
import { Angular5Csv } from 'angular5-csv/Angular5-csv';


@Component({
    templateUrl: './topup-list.component.html',
    styleUrls: ['./topup-list.component.less'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class TopupListComponent extends AppComponentBase implements OnInit {
    fromDate: moment.Moment;
    toDate: moment.Moment;
    machineFilter = '';
    machines: MachineDto[] = [];
    enableExportButton = false;
    currencySymbol = '$';

    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;
    @ViewChild('detailTopupModal') detailTopupModal: TopupDetailComponent;

    constructor(injector: Injector,
        private topupReportService: TopupReportServiceProxy,
        private router: Router,
        private machinesService: MachineServiceProxy) {
        super(injector);
    }

    ngOnInit(): void {
        this.getMachines();

        this.currencySymbol = abp.setting.get("CurrencySymbol");
    }

    loadTopupList(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();
        this.topupReportService.getPagedList(
            undefined,
            this.machineFilter,
            this.fromDate ? moment(this.fromDate) : undefined,
            this.toDate ? moment(this.toDate) : undefined,
            this.primengTableHelper.getSorting(this.dataTable),
            this.primengTableHelper.getSkipCount(this.paginator, event),
            this.primengTableHelper.getMaxResultCount(this.paginator, event)
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
            this.primengTableHelper.hideLoadingIndicator();
            if (result.totalCount >= 10) {
                this.enableExportButton = false;
            } else {
                this.enableExportButton = true;
            }
        });
    }

    exportToCsv() {
        // Check exists data export.
        if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
            // Show icon loading.
            this.primengTableHelper.showLoadingIndicator();
            // Load transactions.
            this.topupReportService.getDataForReport(
                undefined,
                this.machineFilter,
                this.fromDate,
                this.toDate,
                this.primengTableHelper.getSorting(this.dataTable),
                0,
                1000
            ).subscribe(result => {
                if (result.items != null) {
                    // Declare csv data.
                    let csvData = new Array();
                    // Declare const header.
                    const header = {
                        MachineId: 'Machine ID',
                        RestockUnloadId: 'Restock/Unload ID',
                        RestockerId: 'Restocker ID',
                        DateAndTime: 'Date And Time',
                        SessionType: 'Session Type',
                        ItemName: 'Item Name',
                        Quantity: 'Quantity Loaded/Unloaded',
                    };
                    // Add header to csv data.
                    csvData.push(header);
                    // add content file csv.
                    for (let record of result.items) {
                        csvData.push({
                            MachineId: (record.machineId == null) ? '' : record.machineId,
                            RestockUnloadId: (record.itemId == null) ? '' : record.itemId,
                            RestockerId: (record.restockerId == null) ? '' : record.restockerId,
                            DateAndTime: (record.dateTime == null) ? '' : record.dateTime,
                            SessionType: (record.sessionType == null) ? '' : record.sessionType,
                            ItemName: (record.itemName == null) ? '' : record.itemName,
                            Quantity: (record.quantity == null) ? '' : record.quantity,
                        });
                    }
                    // Export csv data.
                    // tslint:disable-next-line:no-unused-expression
                    new Angular5Csv(csvData, 'MagicboxRestockReport', { quoteStrings: '' });
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

    exportTnxItemsCvs(): void {

    }

    showDetailTopupModal(record) {
        this.detailTopupModal.show(record.id);
    }

    getMachines(event?: LazyLoadEvent) {
        this.machinesService.getAll(0, 100, 'name asc')
            .subscribe(result => {
                this.machines = result.items;
            });
    }
}

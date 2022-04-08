import { Component, OnInit, Injector, ViewEncapsulation, ViewChild, Inject, Optional, } from '@angular/core';
import { Router } from '@angular/router';
import { TopupReportServiceProxy } from '@shared/service-proxies/service-proxies';
import { TopupDetailComponent } from './topup-detail.component';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import * as _ from 'lodash';
import * as moment from 'moment';
import { MachineServiceProxy, MachineDto, PagedResultDtoOfMachineDto } from "shared/service-proxies/machine-service-proxies";

@Component({
    templateUrl: './topup-list.component.html',
    styleUrls: ['./topup-list.component.less'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class TopupListComponent extends AppComponentBase implements OnInit {
    fromDate = '';
    toDate = '';

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
    }

    loadTopupList(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();
        this.topupReportService.getPagedList(
            "",
            undefined,
            undefined,
            this.primengTableHelper.getSorting(this.dataTable),
            this.primengTableHelper.getSkipCount(this.paginator, event),
            this.primengTableHelper.getMaxResultCount(this.paginator, event)
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    showDetailTopupModal(record) {
        this.detailTopupModal.show(record.id);
    }
}

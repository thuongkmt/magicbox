import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { ActivatedRoute , Router} from '@angular/router';
import { RestockSessionsServiceProxy, RestockSessionDto  } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';


import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';

@Component({
    templateUrl: './restockSessions.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class RestockSessionsComponent extends AppComponentBase {
    
    
       
    
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;

    advancedFiltersAreShown = false;
    filterText = '';
    maxStartDateFilter : moment.Moment;
		minStartDateFilter : moment.Moment;
    maxEndDateFilter : moment.Moment;
		minEndDateFilter : moment.Moment;
    maxTotalFilter : number;
		maxTotalFilterEmpty : number;
		minTotalFilter : number;
		minTotalFilterEmpty : number;
    maxLeftOverFilter : number;
		maxLeftOverFilterEmpty : number;
		minLeftOverFilter : number;
		minLeftOverFilterEmpty : number;
    maxSoldFilter : number;
		maxSoldFilterEmpty : number;
		minSoldFilter : number;
		minSoldFilterEmpty : number;
    maxErrorFilter : number;
		maxErrorFilterEmpty : number;
		minErrorFilter : number;
		minErrorFilterEmpty : number;
    isProcessingFilter = -1;
    restockerNameFilter = '';




    constructor(
        injector: Injector,
        private _restockSessionsServiceProxy: RestockSessionsServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _activatedRoute: ActivatedRoute,
        private _fileDownloadService: FileDownloadService,
			private _router: Router
    ) {
        super(injector);
    }

    getRestockSessions(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();

        this._restockSessionsServiceProxy.getAll(
            this.filterText,
            this.maxStartDateFilter,
            this.minStartDateFilter,
            this.maxEndDateFilter,
            this.minEndDateFilter,
            this.maxTotalFilter == null ? this.maxTotalFilterEmpty: this.maxTotalFilter,
            this.minTotalFilter == null ? this.minTotalFilterEmpty: this.minTotalFilter,
            this.maxLeftOverFilter == null ? this.maxLeftOverFilterEmpty: this.maxLeftOverFilter,
            this.minLeftOverFilter == null ? this.minLeftOverFilterEmpty: this.minLeftOverFilter,
            this.maxSoldFilter == null ? this.maxSoldFilterEmpty: this.maxSoldFilter,
            this.minSoldFilter == null ? this.minSoldFilterEmpty: this.minSoldFilter,
            this.maxErrorFilter == null ? this.maxErrorFilterEmpty: this.maxErrorFilter,
            this.minErrorFilter == null ? this.minErrorFilterEmpty: this.minErrorFilter,
            this.isProcessingFilter,
            this.restockerNameFilter,
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

    createRestockSession(): void {
        this._router.navigate(['/app/main/restock/restockSessions/createOrEdit']);        
    }


    deleteRestockSession(restockSession: RestockSessionDto): void {
        this.message.confirm(
            '',
            this.l('AreYouSure'),
            (isConfirmed) => {
                if (isConfirmed) {
                    this._restockSessionsServiceProxy.delete(restockSession.id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }

    exportToExcel(): void {
        this._restockSessionsServiceProxy.getRestockSessionsToExcel(
        this.filterText,
            this.maxStartDateFilter,
            this.minStartDateFilter,
            this.maxEndDateFilter,
            this.minEndDateFilter,
            this.maxTotalFilter == null ? this.maxTotalFilterEmpty: this.maxTotalFilter,
            this.minTotalFilter == null ? this.minTotalFilterEmpty: this.minTotalFilter,
            this.maxLeftOverFilter == null ? this.maxLeftOverFilterEmpty: this.maxLeftOverFilter,
            this.minLeftOverFilter == null ? this.minLeftOverFilterEmpty: this.minLeftOverFilter,
            this.maxSoldFilter == null ? this.maxSoldFilterEmpty: this.maxSoldFilter,
            this.minSoldFilter == null ? this.minSoldFilterEmpty: this.minSoldFilter,
            this.maxErrorFilter == null ? this.maxErrorFilterEmpty: this.maxErrorFilter,
            this.minErrorFilter == null ? this.minErrorFilterEmpty: this.minErrorFilter,
            this.isProcessingFilter,
            this.restockerNameFilter,
        )
        .subscribe(result => {
            this._fileDownloadService.downloadTempFile(result);
         });
    }
}

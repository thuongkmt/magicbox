import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Http } from '@angular/http';
import { CreditHistoriesServiceProxy, CreditHistoryDto  } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { CreateOrEditCreditHistoryModalComponent } from './create-or-edit-creditHistory-modal.component';
import { ViewCreditHistoryModalComponent } from './view-creditHistory-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';

@Component({
    templateUrl: './creditHistories.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class CreditHistoriesComponent extends AppComponentBase {

    @ViewChild('createOrEditCreditHistoryModal') createOrEditCreditHistoryModal: CreateOrEditCreditHistoryModalComponent;
    @ViewChild('viewCreditHistoryModalComponent') viewCreditHistoryModal: ViewCreditHistoryModalComponent;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;

    advancedFiltersAreShown = false;
    filterText = '';
    maxValueFilter : number;
		maxValueFilterEmpty : number;
		minValueFilter : number;
		minValueFilterEmpty : number;
    messageFilter = '';
    hashFilter = '';
        userCreditUserIdFilter = '';




    constructor(
        injector: Injector,
        private _creditHistoriesServiceProxy: CreditHistoriesServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _activatedRoute: ActivatedRoute,
        private _fileDownloadService: FileDownloadService
    ) {
        super(injector);
    }

    getCreditHistories(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();

        this._creditHistoriesServiceProxy.getAll(
            this.filterText,
            this.maxValueFilter == null ? this.maxValueFilterEmpty: this.maxValueFilter,
            this.minValueFilter == null ? this.minValueFilterEmpty: this.minValueFilter,
            this.messageFilter,
            this.hashFilter,
            this.userCreditUserIdFilter,
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

    createCreditHistory(): void {
        this.createOrEditCreditHistoryModal.show();
    }

    deleteCreditHistory(creditHistory: CreditHistoryDto): void {
        this.message.confirm(
            '',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._creditHistoriesServiceProxy.delete(creditHistory.id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }

    exportToExcel(): void {
        this._creditHistoriesServiceProxy.getCreditHistoriesToExcel(
        this.filterText,
            this.maxValueFilter == null ? this.maxValueFilterEmpty: this.maxValueFilter,
            this.minValueFilter == null ? this.minValueFilterEmpty: this.minValueFilter,
            this.messageFilter,
            this.hashFilter,
            this.userCreditUserIdFilter,
        )
        .subscribe(result => {
            this._fileDownloadService.downloadTempFile(result);
         });
    }
}

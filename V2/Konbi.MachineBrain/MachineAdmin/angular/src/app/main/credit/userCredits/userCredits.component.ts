import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Http } from '@angular/http';
import { UserCreditsServiceProxy, UserCreditDto  } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { CreateOrEditUserCreditModalComponent } from './create-or-edit-userCredit-modal.component';
import { ViewUserCreditModalComponent } from './view-userCredit-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';

@Component({
    templateUrl: './userCredits.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class UserCreditsComponent extends AppComponentBase {

    @ViewChild('createOrEditUserCreditModal') createOrEditUserCreditModal: CreateOrEditUserCreditModalComponent;
    @ViewChild('viewUserCreditModalComponent') viewUserCreditModal: ViewUserCreditModalComponent;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;

    advancedFiltersAreShown = false;
    filterText = '';
    maxValueFilter : number;
		maxValueFilterEmpty : number;
		minValueFilter : number;
		minValueFilterEmpty : number;
    hashFilter = '';
        userNameFilter = '';




    constructor(
        injector: Injector,
        private _userCreditsServiceProxy: UserCreditsServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _activatedRoute: ActivatedRoute,
        private _fileDownloadService: FileDownloadService
    ) {
        super(injector);
    }

    getUserCredits(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();

        this._userCreditsServiceProxy.getAll(
            this.filterText,
            this.maxValueFilter == null ? this.maxValueFilterEmpty: this.maxValueFilter,
            this.minValueFilter == null ? this.minValueFilterEmpty: this.minValueFilter,
            this.hashFilter,
            this.userNameFilter,
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

    createUserCredit(): void {
        this.createOrEditUserCreditModal.show();
    }

    deleteUserCredit(userCredit: UserCreditDto): void {
        this.message.confirm(
            '',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._userCreditsServiceProxy.delete(userCredit.id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }

    exportToExcel(): void {
        this._userCreditsServiceProxy.getUserCreditsToExcel(
        this.filterText,
            this.maxValueFilter == null ? this.maxValueFilterEmpty: this.maxValueFilter,
            this.minValueFilter == null ? this.minValueFilterEmpty: this.minValueFilter,
            this.hashFilter,
            this.userNameFilter,
        )
        .subscribe(result => {
            this._fileDownloadService.downloadTempFile(result);
         });
    }
}

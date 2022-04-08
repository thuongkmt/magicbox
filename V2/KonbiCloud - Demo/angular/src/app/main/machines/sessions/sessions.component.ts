import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Http } from '@angular/http';
import { SessionsServiceProxy, SessionDto } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { CreateOrEditSessionModalComponent } from './create-or-edit-session-modal.component';
import { ViewSessionModalComponent } from './view-session-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';

@Component({
    templateUrl: './sessions.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class SessionsComponent extends AppComponentBase {

    @ViewChild('createOrEditSessionModal') createOrEditSessionModal: CreateOrEditSessionModalComponent;
    @ViewChild('viewSessionModalComponent') viewSessionModal: ViewSessionModalComponent;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;

    advancedFiltersAreShown = false;
    filterText = '';
    nameFilter = '';
    fromHrsFilter = '';
    toHrsFilter = '';

    constructor(
        injector: Injector,
        private _sessionsServiceProxy: SessionsServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _activatedRoute: ActivatedRoute,
        private _fileDownloadService: FileDownloadService
    ) {
        super(injector);
    }

    getSessions(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();

        this._sessionsServiceProxy.getAll(
            this.filterText,
            this.nameFilter,
            this.fromHrsFilter,
            this.toHrsFilter,
            this.primengTableHelper.getSorting(this.dataTable),
            0,
            1000
            // this.primengTableHelper.getSkipCount(this.paginator, event),
            // this.primengTableHelper.getMaxResultCount(this.paginator, event)
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    reloadPage(): void {
        this.paginator.changePage(this.paginator.getPage());
    }

    createSession(): void {
        this.createOrEditSessionModal.show();
    }

    deleteSession(session: SessionDto): void {
        this.message.confirm(
            '',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._sessionsServiceProxy.delete(session.id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }

    exportToExcel(): void {
        this._sessionsServiceProxy.getSessionsToExcel(
            this.filterText,
            this.nameFilter,
            this.fromHrsFilter,
            this.toHrsFilter,
        )
            .subscribe(result => {
                this._fileDownloadService.downloadTempFile(result);
            });
    }
}
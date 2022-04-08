import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Http } from '@angular/http';
import { PlateCategoryServiceProxy, PlateCategoryDto } from '@shared/service-proxies/plate-category-service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { CreateOrEditPlateCategoryModalComponent } from './create-or-edit-plateCategory-modal.component';
//import { ViewPlateCategoryModalComponent } from './view-plateCategory-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';

@Component({
    templateUrl: './plateCategories.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class PlateCategoriesComponent extends AppComponentBase {

    @ViewChild('createOrEditPlateCategoryModal') createOrEditPlateCategoryModal: CreateOrEditPlateCategoryModalComponent;
    // @ViewChild('viewPlateCategoryModalComponent') viewPlateCategoryModal: ViewPlateCategoryModalComponent;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;

    advancedFiltersAreShown = false;
    filterText = '';
    nameFilter = '';
    descFilter = '';

    constructor(
        injector: Injector,
        private _plateCategoriesServiceProxy: PlateCategoryServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _activatedRoute: ActivatedRoute,
        private _fileDownloadService: FileDownloadService
    ) {
        super(injector);
    }

    getPlateCategories(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();

        this._plateCategoriesServiceProxy.getAll(
            this.filterText,
            this.nameFilter,
            this.primengTableHelper.getSorting(this.dataTable),
            1000,
            0
            // this.primengTableHelper.getMaxResultCount(this.paginator, event),
            // this.primengTableHelper.getSkipCount(this.paginator, event)
        ).subscribe(result => {
            console.log(result);
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;

            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    reloadPage(): void {
        this.paginator.changePage(this.paginator.getPage());
    }

    createPlateCategory(): void {
        this.createOrEditPlateCategoryModal.show();
    }

    deletePlateCategory(plateCategory: PlateCategoryDto): void {
        this.message.confirm(
            '',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._plateCategoriesServiceProxy.delete(plateCategory.id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }

    exportToExcel(): void {
        // this._plateCategoriesServiceProxy.getPlateCategoriesToExcel(
        // this.filterText,
        // 	this.nameFilter,
        // 	this.descFilter,
        // )
        // .subscribe(result => {
        //     this._fileDownloadService.downloadTempFile(result);
        //  });
    }
}
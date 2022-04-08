import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Http } from '@angular/http';
import { CategoryServiceProxy, ProductCategoryDto  } from '@shared/service-proxies/category-service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { CreateOrEditProductCategoryModalComponent } from './create-or-edit-productCategory-modal.component';
import { ViewProductCategoryModalComponent } from './view-productCategory-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';

@Component({
    templateUrl: './productCategories.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class ProductCategoriesComponent extends AppComponentBase {

    @ViewChild('createOrEditProductCategoryModal') createOrEditProductCategoryModal: CreateOrEditProductCategoryModalComponent;
    @ViewChild('viewProductCategoryModalComponent') viewProductCategoryModal: ViewProductCategoryModalComponent;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;

    advancedFiltersAreShown = false;
    filterText = '';
    nameFilter = '';
    codeFilter = '';
    descFilter = '';


    constructor(
        injector: Injector,
        private _productCategoriesServiceProxy: CategoryServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _activatedRoute: ActivatedRoute,
        private _fileDownloadService: FileDownloadService
    ) {
        super(injector);
    }

    getProductCategories(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();

        this._productCategoriesServiceProxy.getAll(
            this.filterText,
            this.nameFilter,
            this.codeFilter,
            this.descFilter,
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

    createProductCategory(): void {
        this.createOrEditProductCategoryModal.show();
    }

    deleteProductCategory(productCategory: ProductCategoryDto): void {
        this.message.confirm(
            '',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._productCategoriesServiceProxy.delete(productCategory.id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }

    exportToExcel(): void {
        this._productCategoriesServiceProxy.getProductCategoriesToExcel(
            this.filterText,
            this.nameFilter,
            this.codeFilter,
            this.descFilter
        )
        .subscribe(result => {
            this._fileDownloadService.downloadTempFile(result);
         });
    }

    // Export list transactions to csv.
    exportToCsv(): void {
        // Check exists data export.
        if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
        // Show icon loading.
        this.primengTableHelper.showLoadingIndicator();
        // Load transactions.
        this._productCategoriesServiceProxy.getAll(
            this.filterText,
            this.nameFilter,
            this.codeFilter,
            this.descFilter,
            this.primengTableHelper.getSorting(this.dataTable),
            0,
            9999999
        ).subscribe(result => {
                if (result.items != null) {
                // Declare csv data.
                let csvData = new Array();
                // Declare const header.
                const header = {
                    Name: 'Name',
                    Code: 'Code',
                    Desc: 'Desc'
                };
                // Add header to csv data.
                csvData.push(header);
                // add content file csv.
                for (let record of result.items) {
                    csvData.push({
                        Name: (record.name == null) ? '' : record.name,
                        Code: (record.code == null) ? '' : record.code,
                        Price: (record.desc == null) ? '' : record.desc
                    });
                }
                // Export csv data.
                new Angular5Csv(csvData, 'MagicBoxProductCategory');
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

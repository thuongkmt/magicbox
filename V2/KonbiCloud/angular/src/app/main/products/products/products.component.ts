import { Component, Injector, ViewEncapsulation, ViewChild, Inject, Optional } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductsServiceProxy } from '@shared/service-proxies/product-service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy, API_BASE_URL, ProductDto } from '@shared/service-proxies/service-proxies';
import { CreateOrEditProductModalComponent } from './create-or-edit-product-modal.component';
import { ViewProductModalComponent } from './view-product-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';
import { ImportProductModelComponent } from '../import-product-model/import-product-model.component';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import { ProductCategoryDto,CategoryServiceProxy } from '@shared/service-proxies/category-service-proxies';

@Component({
    templateUrl: './products.component.html',
    styleUrls: ['./products.component.less'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class ProductsComponent extends AppComponentBase {
    @ViewChild('createOrEditProductModal') createOrEditProductModal: CreateOrEditProductModalComponent;
    @ViewChild('viewProductModalComponent') viewProductModal: ViewProductModalComponent;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;
    @ViewChild('importExcelProductModal') importExcelProductModal: ImportProductModelComponent;

    baseRemoteUrl: string;
    advancedFiltersAreShown = false;
    filterText = '';
    skuFilter = '';
    nameFilter = '';
    maxPriceFilter: number;
    maxPriceFilterEmpty: number;
    minPriceFilter: number;
    minPriceFilterEmpty: number;
    descFilter = '';
    shortDescFilter = '';
    barcodeFilter = '';
    tagFilter = '';
    categoryFilter = '00000000-0000-0000-0000-000000000000';
    categories = new Array<ProductCategoryDto>();

    constructor(
        injector: Injector,
        private _productsServiceProxy: ProductsServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _activatedRoute: ActivatedRoute,
        private _fileDownloadService: FileDownloadService,
        private _categoriesServiceProxy : CategoryServiceProxy,
        @Optional() @Inject(API_BASE_URL) baseRemoteUrl?: string
    ) {
        super(injector);
        this.baseRemoteUrl = baseRemoteUrl ? baseRemoteUrl : "";
        this.getCategories();
    }

    getProducts(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();

        this._productsServiceProxy.getAllProducts(
            this.filterText,
            this.nameFilter,
            this.skuFilter,
            this.barcodeFilter,
            this.shortDescFilter,
            this.descFilter,
            this.tagFilter,
            this.maxPriceFilter,
            this.minPriceFilter,
            this.categoryFilter,
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

    createProduct(): void {
        this.createOrEditProductModal.show();
    }

    deleteProduct(product: ProductDto): void {
        this.message.confirm(
            '',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._productsServiceProxy.delete(product.id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }

    exportToExcel(): void {
        this._productsServiceProxy.getProductsToExcel(
            this.filterText,
            this.nameFilter,
            this.skuFilter,
            this.barcodeFilter,
            this.shortDescFilter,
            this.descFilter,
            this.tagFilter,
            this.maxPriceFilter,
            this.minPriceFilter
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
        this._productsServiceProxy.getAllProducts(
            this.filterText,
            this.nameFilter,
            this.skuFilter,
            this.barcodeFilter,
            this.shortDescFilter,
            this.descFilter,
            this.tagFilter,
            this.maxPriceFilter,
            this.minPriceFilter,
            this.categoryFilter,
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
                SKU: 'SKU',
                Price: 'Price',
                Barcode: 'Barcode',
                Tag: 'Tag',
                ShortDesc: 'ShortDesc',
                Desc: 'Desc',
                ImageUrl: 'ImageUrl',
                Categories: 'Categories'
            };
            // Add header to csv data.
            csvData.push(header);
            // add content file csv.
            for (let record of result.items) {
                let categories = '';
                if(record.categoriesName != null && record.categoriesName.length > 0)
                {
                    for(var i= 0; i < record.categoriesName.length; i++)
                    {
                        categories += record.categoriesName[i];

                        if(i != (record.categoriesName.length -1)){
                            categories += ";";
                        }
                    }
                }

                csvData.push({
                    Name: (record.name == null) ? '' : record.name,
                    SKU: (record.sku == null) ? '' : '_' + record.sku,
                    Price: (record.price == null) ? '' : record.price,
                    Barcode: (record.barcode == null) ? '' :'_' + record.barcode,
                    Tag: (record.tag == null) ? '' : record.tag,
                    ShortDesc: (record.shortDesc == null) ? '' : record.shortDesc,
                    Desc: (record.desc == null) ? '' : record.desc.replace(/<[^>]*>?/gm, ''),
                    ImageUrl: (record.imageUrl == null) ? '' : record.imageUrl,
                    Categories: categories
                });
            }
            // Export csv data.
            // tslint:disable-next-line:no-unused-expression
            new Angular5Csv(csvData, 'MagicBoxProduct',{quoteStrings: ''});
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

    handleImageLoadFailed = (event) => {
        event.target.src = `/assets/images/not_found.png`;
    }

    importProduct() {
        this.importExcelProductModal.show();
    }

    //Get all categories
    getCategories(): void {
        let self = this;
        let categories = [];
        categories.push({id : '00000000-0000-0000-0000-000000000000',name : '-- All --'});
        this._categoriesServiceProxy.getAll("","","","",null,0,1000)
            .subscribe(
                (result) =>
                    result.items.forEach(item => {
                        let category = new ProductCategoryDto();
                        category.id = item.id;
                        category.name = item.name;
                        categories.push(category);
            }));

            self.categories = categories;
    }
}

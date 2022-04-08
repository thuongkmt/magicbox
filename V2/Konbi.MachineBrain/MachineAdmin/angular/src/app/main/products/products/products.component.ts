import { Component, Injector, ViewEncapsulation, ViewChild, Inject, Optional, } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductsServiceProxy, ProductDto } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy, API_BASE_URL } from '@shared/service-proxies/service-proxies';
import { CreateOrEditProductModalComponent } from './create-or-edit-product-modal.component';
import { ViewProductModalComponent } from './view-product-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';
import { ImportProductModalComponent } from './import-product-modal.component';
import { SystemConfigServiceProxy, SystemConfigDto, PagedResultDtoOfSystemConfigDto } from '@shared/service-proxies/system-config-service-proxies';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import { ProductCategoryDto, ProductCategoriesServiceProxy } from '@shared/service-proxies/service-proxies';

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
    @ViewChild('importExcelProductModal') importExcelProductModal: ImportProductModalComponent;

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
    cloudApiServerUrl: string;
    categoryFilter = '00000000-0000-0000-0000-000000000000';
    categories = new Array<ProductCategoryDto>();

    constructor(
        injector: Injector,
        private _productsServiceProxy: ProductsServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _activatedRoute: ActivatedRoute,
        private _fileDownloadService: FileDownloadService,
        private systemConfigService: SystemConfigServiceProxy,
        private _categoriesServiceProxy: ProductCategoriesServiceProxy,
        @Optional() @Inject(API_BASE_URL) baseRemoteUrl?: string,
    ) {
        super(injector);
        this.baseRemoteUrl = baseRemoteUrl ? baseRemoteUrl : "";

        this.systemConfigService.getAllByName("RfidFridgeSetting")
            .subscribe((result: PagedResultDtoOfSystemConfigDto) => {
                let allSetting = result.items;
                let cloudApiUrlSetting = allSetting.find(x => x.name == "RfidFridgeSetting.System.Cloud.CloudApiUrl");
                this.cloudApiServerUrl = cloudApiUrlSetting == null ? "" : cloudApiUrlSetting.value;
            });
        this.getCategories();
    }

    getProducts(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();
        this._productsServiceProxy.getAll(
            this.filterText,
            this.nameFilter,
            this.skuFilter,
            this.barcodeFilter,
            this.shortDescFilter,
            this.descFilter,
            this.tagFilter,
            this.categoryFilter,
            this.maxPriceFilter,
            this.minPriceFilter,
            "",
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
            this.skuFilter,

            this.nameFilter,
            this.maxPriceFilter,
            this.minPriceFilter,
            this.descFilter,
            this.shortDescFilter,
            ""
        )
            .subscribe(result => {
                this._fileDownloadService.downloadTempFile(result);
            });
    }

    handleImageLoadFailed = (event) => {
        event.target.src = `/assets/images/not_found.png`;
    }

    importProduct() {
        this.importExcelProductModal.show();
    }

    exportToCsv(): void {
        // Check exists data export.
        if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
            // Show icon loading.
            this.primengTableHelper.showLoadingIndicator();
            // Load transactions.
            this._productsServiceProxy.getAll(
                this.filterText,
                this.nameFilter,
                this.skuFilter,
                this.barcodeFilter,
                this.shortDescFilter,
                this.descFilter,
                this.tagFilter,
                this.categoryFilter,
                this.maxPriceFilter,
                this.minPriceFilter,
                "",
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
                        if (record.product.categoriesName != null && record.product.categoriesName.length > 0) {
                            for (var i = 0; i < record.product.categoriesName.length; i++) {
                                categories += record.product.categoriesName[i];

                                if (i != (record.product.categoriesName.length - 1)) {
                                    categories += ";";
                                }
                            }
                        }
                        csvData.push({
                            Name: (record.product.name == null) ? '' : record.product.name,
                            SKU: (record.product.sku == null) ? '' : '_' + record.product.sku,
                            Price: (record.product.price == null) ? '' : record.product.price,
                            Barcode: (record.product.barcode == null) ? '' : '_' + record.product.barcode,
                            Tag: (record.product.tag == null) ? '' : record.product.tag,
                            ShortDesc: (record.product.shortDesc == null) ? '' : record.product.shortDesc,
                            Desc: (record.product.desc == null) ? '' : record.product.desc.replace(/<[^>]*>?/gm, ''),
                            ImageUrl: (record.product.imageUrl == null) ? '' : record.product.imageUrl,
                            Categories: categories
                        });
                    }
                    // Export csv data.
                    // tslint:disable-next-line:no-unused-expression
                    new Angular5Csv(csvData, 'MagicBoxProduct', { quoteStrings: '' });
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


    syncFromCloud(): void {
        this.message.confirm(
            '',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._productsServiceProxy.sendSyncProductsRequest()
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success('Successfully synchronize');
                        });
                }
            }
        );
    }

    //Get all categories
    getCategories(): void {
        let self = this;
        let categories = [];
        categories.push({ id: '00000000-0000-0000-0000-000000000000', name: '-- All --' });
        this._categoriesServiceProxy.getAll("", "", "", "", null, 0, 1000)
            .subscribe(
                (result) =>
                    result.items.forEach(item => {
                        let category = new ProductCategoryDto();
                        category.id = item.productCategory.id;
                        category.name = item.productCategory.name;
                        categories.push(category);
                    }));

        self.categories = categories;
    }
}

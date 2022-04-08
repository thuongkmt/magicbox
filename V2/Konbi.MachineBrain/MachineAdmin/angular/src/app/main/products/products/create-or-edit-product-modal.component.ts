import { Component, ViewChild, Injector, Output, EventEmitter, Inject, Optional } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { ProductsServiceProxy, CreateOrEditProductDto, API_BASE_URL,ProductCategoriesServiceProxy, PagedResultDtoOfGetProductCategoryForViewDto,GetProductCategoryForViewDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import * as moment from 'moment';
import { RxStompService } from '@stomp/ng2-stompjs';
import {EditorModule} from 'primeng/editor';
import { SystemConfigServiceProxy, SystemConfigDto, PagedResultDtoOfSystemConfigDto } from '@shared/service-proxies/system-config-service-proxies';

@Component({
    selector: 'createOrEditProductModal',
    templateUrl: './create-or-edit-product-modal.component.html'
})
export class CreateOrEditProductModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
    baseRemoteUrl: string;

    active = false;
    saving = false;
    isValidProductPrice = true;

    product: CreateOrEditProductDto = new CreateOrEditProductDto();
    categories: SelectionCategory[] = [];
    productImgThumb: string = null;
    cloudApiServerUrl: string;


    constructor(
        injector: Injector,
        private _productsServiceProxy: ProductsServiceProxy,
        private _productCategoriesServiceProxy : ProductCategoriesServiceProxy,
        private rxStompService: RxStompService,
        private systemConfigService: SystemConfigServiceProxy,
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
    }

    show(productId?: string): void {
        this.productImgThumb = null;
        if (!productId) {
            this.product = new CreateOrEditProductDto();
            this.product.id = productId;
            this.active = true;
            this.getCategories();
            this.modal.show();
        } else {
            this._productsServiceProxy.getProductForEdit(productId).subscribe(result => {
                this.product = result.product;
                if(this.product.imageUrl){
                    let productImgThumbPicture = 'thumb_' + this.product.imageUrl;
                    this.productImgThumb = `${this.cloudApiServerUrl}/images/${productImgThumbPicture}`;
                }
                this.getProductCategories(this.product.categoryIds);
                this.active = true;
                this.modal.show();
            });
        }
    }

    refreshProduct() {
        this.rxStompService.publish({ destination: '/topic/command', body: 'MACHINE_REFRESHPRD' });
    }

    save(): void {
        this.saving = true;
        this.product.categoryIds = this.selectedCategories;
        this._productsServiceProxy.createOrEdit(this.product)
            .pipe(finalize(() => { this.saving = false; }))
            .subscribe(() => {
                this.refreshProduct();
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
            });
    }

    handleSelectImageUrl = (data) => {
        this.productImgThumb = `${this.cloudApiServerUrl}/images/${data[0].thumbnail}`;
        this.product.imageUrl = data[0].fileName;
    }

    handleImageLoadFailed = (event) => {
        event.target.src = `/assets/images/not_found.png`;
    }
    
    close(): void {

        this.active = false;
        this.modal.hide();
    }

    getCategories(): void {
        let self = this;
        let categories = [];
        this._productCategoriesServiceProxy.getAll("","","","",null,0,1000)
            .subscribe(
                (result : PagedResultDtoOfGetProductCategoryForViewDto) =>
                    result.items.forEach(item => {
                        let category = new SelectionCategory(item.productCategory.id,item.productCategory.name,false);
                        categories.push(category);
            }));
            self.categories = categories;
    }

    getProductCategories(productCategoryIds : string[]): void {
        let self = this;
        let categories = [];
        this._productCategoriesServiceProxy.getAll("","","","",null,0,1000)
            .subscribe(
                (result : PagedResultDtoOfGetProductCategoryForViewDto) =>
                    result.items.forEach(item => {
                        if(productCategoryIds.find(x => x === item.productCategory.id)){
                            categories.push(new SelectionCategory(item.productCategory.id,item.productCategory.name,true));
                        }
                        else{
                            categories.push(new SelectionCategory(item.productCategory.id,item.productCategory.name,false));
                        }
            }));

        self.categories = categories;
    }

    get selectedCategories() {
        return this.categories
          .filter(opt => opt.checked)
          .map(opt => opt.id)
      }

    validateProductPrice() {
        if(this.product.price < 0.01){
            this.isValidProductPrice = false;
        }
        else{
            this.isValidProductPrice = true;
        }
    }
}


export class SelectionCategory {
    name: string;
    id: string;
    checked: boolean;
  
    constructor(id: string, name: string, checked: boolean) {
      this.id = id;
      this.name = name;
      this.checked = checked;
    }
}

import { Component, ViewChild, Injector, Output, EventEmitter, Inject, Optional, } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { API_BASE_URL, CreateOrEditProductDto} from '@shared/service-proxies/service-proxies';
import {  SelectionCategory, CategoryServiceProxy, PagedResultDtoOfGetProductCategoryForViewDto, CategoryDto} from '@shared/service-proxies/category-service-proxies';
import { ProductsServiceProxy} from '@shared/service-proxies/product-service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import * as moment from 'moment';
import {EditorModule} from 'primeng/editor';

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

    constructor(
        injector: Injector,
        private _productsServiceProxy: ProductsServiceProxy,
        private _categoriesServiceProxy : CategoryServiceProxy,
        @Optional() @Inject(API_BASE_URL) baseRemoteUrl?: string,
    ) {
        super(injector);
        this.baseRemoteUrl = baseRemoteUrl ? baseRemoteUrl : "";
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
                    this.productImgThumb = `${this.baseRemoteUrl}/images/${productImgThumbPicture}`;
                }
                this.getProductCategories(this.product.categoryIds);
                this.active = true;
                this.modal.show();
            });
        }
    }

    save(): void {
        this.saving = true;
        this.product.categoryIds = this.selectedCategories;
        this._productsServiceProxy.createOrEdit(this.product)
            .pipe(finalize(() => { this.saving = false; }))
            .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
            });
    }

    handleSelectImageUrl = (data) => {
        this.productImgThumb = `${this.baseRemoteUrl}/images/${data[0].thumbnail}`;
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
        this._categoriesServiceProxy.getAll("","","","",null,0,1000)
            .subscribe(
                (result) =>
                    result.items.forEach(item => {
                        let category = new SelectionCategory(item.id,item.name,false);
                        categories.push(category);
            }));
            self.categories = categories;
    }

    getProductCategories(productCategoryIds : string[]): void {
        let self = this;
        let categories = [];
        this._categoriesServiceProxy.getAll("","","","",null,0,1000)
            .subscribe(
                (result) =>
                    result.items.forEach(item => {
                        if(productCategoryIds.find(x => x === item.id)){
                            categories.push(new SelectionCategory(item.id,item.name,true));
                        }
                        else{
                            categories.push(new SelectionCategory(item.id,item.name,false));
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

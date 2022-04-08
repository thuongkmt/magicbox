import { Component, ViewChild, Injector, Output, EventEmitter} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { ProductCategoriesServiceProxy, CreateOrEditProductCategoryDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import * as moment from 'moment';


@Component({
    selector: 'createOrEditProductCategoryModal',
    templateUrl: './create-or-edit-productCategory-modal.component.html'
})
export class CreateOrEditProductCategoryModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;


    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    productCategory: CreateOrEditProductCategoryDto = new CreateOrEditProductCategoryDto();



    constructor(
        injector: Injector,
        private _productCategoriesServiceProxy: ProductCategoriesServiceProxy
    ) {
        super(injector);
    }

    show(productCategoryId?: string): void {
        if (!productCategoryId) {
            this.productCategory = new CreateOrEditProductCategoryDto();
            this.productCategory.id = productCategoryId;

            this.active = true;
            this.modal.show();
        } else {
            this._productCategoriesServiceProxy.getProductCategoryForEdit(productCategoryId).subscribe(result => {
                this.productCategory = result.productCategory;
                this.active = true;
                this.modal.show();
            });
        }
    }

    save(): void {
            this.saving = true;
            this._productCategoriesServiceProxy.createOrEdit(this.productCategory)
             .pipe(finalize(() => { this.saving = false;}))
             .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
             });
    }

    close(): void {

        this.active = false;
        this.modal.hide();
    }
}

import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { ProductCategoryDto } from '@shared/service-proxies/category-service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
    selector: 'viewProductCategoryModal',
    templateUrl: './view-productCategory-modal.component.html'
})
export class ViewProductCategoryModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    item: ProductCategoryDto;


    constructor(
        injector: Injector
    ) {
        super(injector);
        this.item = new ProductCategoryDto();
    }

    show(item: ProductCategoryDto): void {
        this.item = item;
        this.active = true;
        this.modal.show();
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }
}

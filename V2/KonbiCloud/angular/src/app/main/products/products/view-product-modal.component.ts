import { Component, ViewChild, Injector, Output, EventEmitter, Optional,Inject, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { API_BASE_URL, ProductDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
    selector: 'viewProductModal',
    templateUrl: './view-product-modal.component.html'
})
export class ViewProductModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @ViewChild('productDescriptionContainer') productDescriptionContainer: ElementRef;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;
    baseRemoteUrl: string;
    productImgThumb: string = null;
    item: ProductDto;


    constructor(
        injector: Injector,
        @Optional() @Inject(API_BASE_URL) baseRemoteUrl?: string,
    ) {
        super(injector);
        this.item = new ProductDto();
        this.baseRemoteUrl = baseRemoteUrl ? baseRemoteUrl : "";
    }

    show(item: ProductDto): void {
        this.item = item;
        if(this.item.imageUrl){
            let productImgThumbPicture = 'thumb_' + this.item.imageUrl;
            this.productImgThumb = `${this.baseRemoteUrl}/images/${productImgThumbPicture}`;
        }
        else
        {
            this.productImgThumb = `/assets/images/no-image.png`;
        }

        this.productDescriptionContainer.nativeElement.innerHTML = this.item.desc;
        this.active = true;
        this.modal.show();
    }

    handleImageLoadFailed = (event) => {
        event.target.src = `/assets/images/not_found.png`;
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }
}

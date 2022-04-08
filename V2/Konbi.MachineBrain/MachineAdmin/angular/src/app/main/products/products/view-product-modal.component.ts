import { Component, ViewChild, Injector, Output, EventEmitter,Optional,Inject, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { GetProductForViewDto, ProductDto, API_BASE_URL} from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { SystemConfigServiceProxy, SystemConfigDto, PagedResultDtoOfSystemConfigDto } from '@shared/service-proxies/system-config-service-proxies';

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
    item: GetProductForViewDto;
    cloudApiServerUrl: string;

    constructor(
        injector: Injector,
        private systemConfigService: SystemConfigServiceProxy,
        @Optional() @Inject(API_BASE_URL) baseRemoteUrl?: string,
    ) {
        super(injector);
        this.item = new GetProductForViewDto();
        this.item.product = new ProductDto();
        this.baseRemoteUrl = baseRemoteUrl ? baseRemoteUrl : "";
        
        this.systemConfigService.getAllByName("RfidFridgeSetting")
        .subscribe((result: PagedResultDtoOfSystemConfigDto) => {
            let allSetting = result.items;
            let cloudApiUrlSetting = allSetting.find(x => x.name == "RfidFridgeSetting.System.Cloud.CloudApiUrl");
            this.cloudApiServerUrl = cloudApiUrlSetting == null ? "" : cloudApiUrlSetting.value;
        });
    }

    show(item: GetProductForViewDto): void {
        this.item = item;
        if(this.item.product.imageUrl){
            let productImgThumbPicture = 'thumb_' + this.item.product.imageUrl;
            this.productImgThumb = `${this.cloudApiServerUrl}/images/${productImgThumbPicture}`;
        }
        else
        {
            this.productImgThumb = `/assets/images/no-image.png`;
        }

        this.productDescriptionContainer.nativeElement.innerHTML = this.item.product.desc;
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

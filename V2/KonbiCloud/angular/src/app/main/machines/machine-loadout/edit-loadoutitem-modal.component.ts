import { Component, ViewChild, Injector, Output, EventEmitter} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ProductsServiceProxy } from '@shared/service-proxies/product-service-proxies';
import { ProductDto } from '@shared/service-proxies/service-proxies';
import { MachineLoadoutServiceProxy, LoadoutDto, LoadoutItem, LoadoutList, UpdateLoadoutDto } from '@shared/service-proxies/machineLoadout-service-proxies';
import * as lodash from 'lodash';

@Component({
    selector: 'editLoadoutItemModalSelector',
    templateUrl: './edit-loadoutitem-modal.component.html'
})
export class EditLoadoutItemModal extends AppComponentBase {

    @ViewChild('editLoadoutItemModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    products: ProductDto[] = [];
    selectedProduct: ProductDto = new ProductDto();
    machineId: any;
    selectedItem: LoadoutItem = new LoadoutItem();
    productId: string = "";
    currencySymbol: string = "";
    baseRemoteUrl: string = "";

    constructor(
        injector: Injector,
        private productService: ProductsServiceProxy,
        private loadoutService: MachineLoadoutServiceProxy,
    ) {
        super(injector);
    }

    show(selectedItem, baseRemoteUrl, machineId): void {
        abp.ui.setBusy();
        this.machineId = machineId;
        this.selectedProduct = new ProductDto();
        this.currencySymbol = abp.setting.get("CurrencySymbol");
        this.selectedItem = lodash.cloneDeep(selectedItem);
        this.productId = "";
        this.baseRemoteUrl = baseRemoteUrl;

        if(this.selectedItem != null)
        {
            this.productId = this.selectedItem.productId;
        }

        this.productService.getAllProductsNoFilter()
            .finally(() => {
                abp.ui.clearBusy();
                this.active = true;
                this.modal.show();
            })
            .subscribe((result) => {
                this.products = result.items;
                if(this.productId != undefined && this.productId.length > 0)
                {
                    this.selectedProduct = this.products.find(x => x.id == this.productId);
                }
                
                abp.ui.clearBusy();
                this.active = true;
                this.modal.show();
            });

        
    }

    save(): void {
        if(this.selectedItem.quantity > this.selectedItem.capacity){
            this.message.error("Quantity cannot greater than capacity");
            return;
        }
        abp.ui.setBusy();
        this.saving = true;
        this.loadoutService.updateLoadoutItem(this.selectedItem)
        .finally(() => {
            this.saving = false;
            abp.ui.clearBusy();
        })
        .subscribe(() => {
            abp.ui.clearBusy();
            this.notify.info(this.l('SavedSuccessfully'));
            this.close();
            this.modalSave.emit(null);
        });
    }

    close(): void
    {
        this.active = false;
        this.modal.hide();
    }

    onSelectProductChange() {
        if(this.productId == "")
        {
            this.selectedProduct = new ProductDto();
            this.selectedItem.productId = this.productId;
            this.selectedItem.price = 0;
            this.selectedItem.quantity = 0;
            return;
        }

        this.selectedProduct = this.products.find(x => x.id == this.productId);
        this.selectedItem.productId = this.productId;
        this.selectedItem.price = this.selectedProduct.price;
    }

}

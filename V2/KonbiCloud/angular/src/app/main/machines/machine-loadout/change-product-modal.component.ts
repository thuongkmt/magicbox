import { Component, ViewChild, Injector, Output, EventEmitter} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ProductsServiceProxy } from '@shared/service-proxies/product-service-proxies';
import { ProductDto } from '@shared/service-proxies/service-proxies';
import { MachineLoadoutServiceProxy, LoadoutDto, LoadoutItem, LoadoutList, UpdateLoadoutDto } from '@shared/service-proxies/machineLoadout-service-proxies';
import * as lodash from 'lodash';

@Component({
    selector: 'changeProductModalSelector',
    templateUrl: './change-product-modal.component.html'
})
export class ChangeProductModal extends AppComponentBase {

    @ViewChild('changeProductModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    products: ProductDto[] = [];
    selectedProduct: ProductDto = new ProductDto();
    currentMachineID: any;
    productId: string = "";
    currencySymbol: string = "";
    updateSlotsPrice: boolean = false;
    loadout: LoadoutDto = new LoadoutDto();
    baseRemoteUrl: string = "";

    constructor(
        injector: Injector,
        private productService: ProductsServiceProxy,
        private loadoutService: MachineLoadoutServiceProxy,
    ) {
        super(injector);
    }

    show(loadout, baseRemoteUrl): void {
        abp.ui.setBusy();
        this.updateSlotsPrice = false;
        this.selectedProduct = new ProductDto();
        this.currencySymbol = abp.setting.get("CurrencySymbol");
        this.productId = "";
        this.loadout = lodash.cloneDeep(loadout)
        this.baseRemoteUrl = baseRemoteUrl;

        this.productService.getAllProductsNoFilter()
            .finally(() => {
                abp.ui.clearBusy();
                this.active = true;
                this.modal.show();
            })
            .subscribe((result) => {
                this.products = result.items;
                if(this.productId != null && this.productId != undefined && this.productId.length > 0)
                {
                    this.onSelectProductChange();
                }
                abp.ui.clearBusy();
                this.active = true;
                this.modal.show();
            });

        
    }

    save(): void {
        abp.ui.setBusy();
        var updateDto = new UpdateLoadoutDto();
        updateDto.machineId = this.loadout.machineId;
        updateDto.loadouts = [];
        this.loadout.loadoutList.forEach(item =>
        {
            item.loadouts.forEach(loadout => 
            {
                if(loadout.isChecked)
                {
                    if(loadout.productId != this.selectedProduct.id ||
                        (this.updateSlotsPrice && loadout.price != this.selectedProduct.price))
                    {
                        loadout.productId = this.selectedProduct.id;
                        if(this.updateSlotsPrice)
                        {
                            loadout.price = this.selectedProduct.price;
                        }
                        updateDto.loadouts.push(loadout);
                    }
                }
                else
                {
                    if(loadout.productId == this.selectedProduct.id)
                    {
                        loadout.productId = null;
                        loadout.price = 0;
                        loadout.quantity = 0;
                        updateDto.loadouts.push(loadout);
                    }
                }
            });
        });
        if(updateDto.loadouts.length > 0)
        {
            this.saving = true;
            this.loadoutService.updateLoadout(updateDto)
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
        else
        {
            abp.ui.clearBusy();
        }
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
            this.loadout.loadoutList.forEach(item =>
                {
                    item.loadouts.forEach(loadout => 
                    {
                        loadout.isChecked = false
                    });
                });
            return;
        }

        this.selectedProduct = this.products.find(x => x.id == this.productId);
        this.loadout.loadoutList.forEach(item =>
        {
            item.loadouts.forEach(loadout => 
            {
                if(loadout.productId == this.selectedProduct.id)
                {
                    loadout.isChecked = true
                }
                else
                {
                    loadout.isChecked = false
                }
            });
        });
    }

    checkAllChange(item: LoadoutList)
    {
        item.loadouts.forEach(lo => 
        {
            lo.isChecked = item.checkAll;
        });
    }
}

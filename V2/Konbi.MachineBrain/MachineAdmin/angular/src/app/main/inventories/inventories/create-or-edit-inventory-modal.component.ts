import { Component, ViewChild, Injector, Output, EventEmitter} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { InventoriesServiceProxy, CreateOrEditInventoryDto } from '@shared/service-proxies/inventories-service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import * as moment from 'moment';
import { ProductLookupTableModalComponent } from './product-lookup-table-modal.component';


@Component({
    selector: 'createOrEditInventoryModal',
    templateUrl: './create-or-edit-inventory-modal.component.html'
})
export class CreateOrEditInventoryModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @ViewChild('productLookupTableModal') productLookupTableModal: ProductLookupTableModalComponent;


    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    inventory: CreateOrEditInventoryDto = new CreateOrEditInventoryDto();

    productName = '';


    constructor(
        injector: Injector,
        private _inventoriesServiceProxy: InventoriesServiceProxy
    ) {
        super(injector);
    }

    show(inventoryId?: string): void {

        if (!inventoryId) {
            this.inventory = new CreateOrEditInventoryDto();
            this.inventory.id = inventoryId;
            this.productName = '';

            this.active = true;
            this.modal.show();
        } else {
            this._inventoriesServiceProxy.getInventoryForEdit(inventoryId).subscribe(result => {
                this.inventory = result.inventory;

                this.productName = result.productName;

                this.active = true;
                this.modal.show();
            });
        }
    }

    save(): void {
            this.saving = true;

			
            this._inventoriesServiceProxy.createOrEdit(this.inventory)
             .pipe(finalize(() => { this.saving = false;}))
             .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
             });
    }

        openSelectProductModal() {
        this.productLookupTableModal.id = this.inventory.productId;
        this.productLookupTableModal.displayName = this.productName;
        this.productLookupTableModal.show();
    }


        setProductIdNull() {
        this.inventory.productId = null;
        this.productName = '';
    }


        getNewProductId() {
        this.inventory.productId = this.productLookupTableModal.id;
        this.productName = this.productLookupTableModal.displayName;
    }


    close(): void {

        this.active = false;
        this.modal.hide();
    }
}

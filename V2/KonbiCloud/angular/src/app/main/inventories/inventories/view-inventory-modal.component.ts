import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { GetInventoryForViewDto, InventoryDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
    selector: 'viewInventoryModal',
    templateUrl: './view-inventory-modal.component.html'
})
export class ViewInventoryModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    item: GetInventoryForViewDto;


    constructor(
        injector: Injector
    ) {
        super(injector);
        this.item = new GetInventoryForViewDto();
        this.item.inventory = new InventoryDto();
    }

    show(item: GetInventoryForViewDto): void {
        this.item = item;
        this.active = true;
        this.modal.show();
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }
}

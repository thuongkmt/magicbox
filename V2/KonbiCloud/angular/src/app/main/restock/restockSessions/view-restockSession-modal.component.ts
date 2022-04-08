import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { GetRestockSessionForViewDto, RestockSessionDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
    selector: 'viewRestockSessionModal',
    templateUrl: './view-restockSession-modal.component.html'
})
export class ViewRestockSessionModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    item: GetRestockSessionForViewDto;


    constructor(
        injector: Injector
    ) {
        super(injector);
        this.item = new GetRestockSessionForViewDto();
        this.item.restockSession = new RestockSessionDto();
    }

    show(item: GetRestockSessionForViewDto): void {
        this.item = item;
        this.active = true;
        this.modal.show();
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }
}

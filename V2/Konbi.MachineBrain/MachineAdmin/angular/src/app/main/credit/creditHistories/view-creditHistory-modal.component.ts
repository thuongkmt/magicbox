import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { GetCreditHistoryForViewDto, CreditHistoryDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
    selector: 'viewCreditHistoryModal',
    templateUrl: './view-creditHistory-modal.component.html'
})
export class ViewCreditHistoryModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    item: GetCreditHistoryForViewDto;


    constructor(
        injector: Injector
    ) {
        super(injector);
        this.item = new GetCreditHistoryForViewDto();
        this.item.creditHistory = new CreditHistoryDto();
    }

    show(item: GetCreditHistoryForViewDto): void {
        this.item = item;
        this.active = true;
        this.modal.show();
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }
}

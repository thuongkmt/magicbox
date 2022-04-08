import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { GetUserCreditForViewDto, UserCreditDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
    selector: 'viewUserCreditModal',
    templateUrl: './view-userCredit-modal.component.html'
})
export class ViewUserCreditModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    item: GetUserCreditForViewDto;


    constructor(
        injector: Injector
    ) {
        super(injector);
        this.item = new GetUserCreditForViewDto();
        this.item.userCredit = new UserCreditDto();
    }

    show(item: GetUserCreditForViewDto): void {
        this.item = item;
        this.active = true;
        this.modal.show();
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }
}

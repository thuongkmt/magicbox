import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { GetSessionForView, SessionDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';

@Component({
    selector: 'viewSessionModal',
    templateUrl: './view-session-modal.component.html'
})
export class ViewSessionModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;


    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    item : GetSessionForView;
	

    constructor(
        injector: Injector
    ) {
        super(injector);
        this.item = new GetSessionForView();
        this.item.session = new SessionDto();
    }

    show(item: GetSessionForView): void {
        this.item = item;
        this.active = true;
        this.modal.show();
    }
    
    close(): void {
        this.active = false;
        this.modal.hide();
    }
}

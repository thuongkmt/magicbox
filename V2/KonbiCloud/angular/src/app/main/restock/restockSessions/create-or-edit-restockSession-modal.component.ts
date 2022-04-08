import { Component, ViewChild, Injector, Output, EventEmitter} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { RestockSessionsServiceProxy, CreateOrEditRestockSessionDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import * as moment from 'moment';

@Component({
    selector: 'createOrEditRestockSessionModal',
    templateUrl: './create-or-edit-restockSession-modal.component.html'
})
export class CreateOrEditRestockSessionModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    restockSession: CreateOrEditRestockSessionDto = new CreateOrEditRestockSessionDto();



    constructor(
        injector: Injector,
        private _restockSessionsServiceProxy: RestockSessionsServiceProxy
    ) {
        super(injector);
    }

    show(restockSessionId?: number): void {

        if (!restockSessionId) {
            this.restockSession = new CreateOrEditRestockSessionDto();
            this.restockSession.id = restockSessionId;
            this.restockSession.startDate = moment().startOf('day');
            this.restockSession.endDate = moment().startOf('day');

            this.active = true;
            this.modal.show();
        } else {
            this._restockSessionsServiceProxy.getRestockSessionForEdit(restockSessionId).subscribe(result => {
                this.restockSession = result.restockSession;


                this.active = true;
                this.modal.show();
            });
        }
        
    }

    save(): void {
            this.saving = true;

			
            this._restockSessionsServiceProxy.createOrEdit(this.restockSession)
             .pipe(finalize(() => { this.saving = false;}))
             .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
             });
    }







    close(): void {
        this.active = false;
        this.modal.hide();
    }
}

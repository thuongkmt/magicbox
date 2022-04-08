import { Component, ViewChild, Injector, Output, EventEmitter} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { SessionsServiceProxy, CreateOrEditSessionDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';


@Component({
    selector: 'createOrEditSessionModal',
    templateUrl: './create-or-edit-session-modal.component.html'
})
export class CreateOrEditSessionModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
	

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    session: CreateOrEditSessionDto = new CreateOrEditSessionDto();
	

    constructor(
        injector: Injector,
        private _sessionsServiceProxy: SessionsServiceProxy
    ) {
        super(injector);
    }

    show(sessionId?: string): void {
        if (!sessionId) { 
			this.session = new CreateOrEditSessionDto();
			this.session.id = sessionId;
			
			this.active = true;
			this.modal.show();
        }
		else{
			this._sessionsServiceProxy.getSessionForEdit(sessionId).subscribe(result => {
				this.session = result.session;
				
				this.active = true;
				this.modal.show();
			});
		}  
    }

    save(): void {
			this.saving = true;
			this._sessionsServiceProxy.createOrEdit(this.session)
			 .pipe(finalize(() => { this.saving = false; }))
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
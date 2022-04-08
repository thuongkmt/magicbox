import { Component, ViewChild, Injector, Output, EventEmitter} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { UserCreditsServiceProxy, CreateOrEditUserCreditDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import * as moment from 'moment';
import { UserLookupTableModalComponent } from './user-lookup-table-modal.component';


@Component({
    selector: 'createOrEditUserCreditModal',
    templateUrl: './create-or-edit-userCredit-modal.component.html'
})
export class CreateOrEditUserCreditModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @ViewChild('userLookupTableModal') userLookupTableModal: UserLookupTableModalComponent;


    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    userCredit: CreateOrEditUserCreditDto = new CreateOrEditUserCreditDto();

    userName = '';


    constructor(
        injector: Injector,
        private _userCreditsServiceProxy: UserCreditsServiceProxy
    ) {
        super(injector);
    }

    show(userCreditId?: string): void {

        if (!userCreditId) {
            this.userCredit = new CreateOrEditUserCreditDto();
            this.userCredit.id = userCreditId;
            this.userName = '';

            this.active = true;
            this.modal.show();
        } else {
            this._userCreditsServiceProxy.getUserCreditForEdit(userCreditId).subscribe(result => {
                this.userCredit = result.userCredit;

                this.userName = result.userName;

                this.active = true;
                this.modal.show();
            });
        }
    }

    save(): void {
            this.saving = true;

			
            this._userCreditsServiceProxy.createOrEdit(this.userCredit)
             .pipe(finalize(() => { this.saving = false;}))
             .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
             });
    }

        openSelectUserModal() {
        this.userLookupTableModal.id = this.userCredit.userId;
        this.userLookupTableModal.displayName = this.userName;
        this.userLookupTableModal.show();
    }


        setUserIdNull() {
        this.userCredit.userId = null;
        this.userName = '';
    }


        getNewUserId() {
        this.userCredit.userId = this.userLookupTableModal.id;
        this.userName = this.userLookupTableModal.displayName;
    }


    close(): void {

        this.active = false;
        this.modal.hide();
    }
}

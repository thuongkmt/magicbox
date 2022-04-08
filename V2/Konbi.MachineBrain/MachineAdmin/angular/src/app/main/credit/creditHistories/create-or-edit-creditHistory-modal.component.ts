import { Component, ViewChild, Injector, Output, EventEmitter} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { CreditHistoriesServiceProxy, CreateOrEditCreditHistoryDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import * as moment from 'moment';
import { UserCreditLookupTableModalComponent } from './userCredit-lookup-table-modal.component';


@Component({
    selector: 'createOrEditCreditHistoryModal',
    templateUrl: './create-or-edit-creditHistory-modal.component.html'
})
export class CreateOrEditCreditHistoryModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @ViewChild('userCreditLookupTableModal') userCreditLookupTableModal: UserCreditLookupTableModalComponent;


    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    creditHistory: CreateOrEditCreditHistoryDto = new CreateOrEditCreditHistoryDto();

    userCreditUserId = '';


    constructor(
        injector: Injector,
        private _creditHistoriesServiceProxy: CreditHistoriesServiceProxy
    ) {
        super(injector);
    }

    show(creditHistoryId?: string): void {

        if (!creditHistoryId) {
            this.creditHistory = new CreateOrEditCreditHistoryDto();
            this.creditHistory.id = creditHistoryId;
            this.userCreditUserId = '';

            this.active = true;
            this.modal.show();
        } else {
            this._creditHistoriesServiceProxy.getCreditHistoryForEdit(creditHistoryId).subscribe(result => {
                this.creditHistory = result.creditHistory;

                this.userCreditUserId = result.userCreditUserId;

                this.active = true;
                this.modal.show();
            });
        }
    }

    save(): void {
            this.saving = true;

			
            this._creditHistoriesServiceProxy.createOrEdit(this.creditHistory)
             .pipe(finalize(() => { this.saving = false;}))
             .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
             });
    }

        openSelectUserCreditModal() {
        this.userCreditLookupTableModal.id = this.creditHistory.userCreditId;
        this.userCreditLookupTableModal.displayName = this.userCreditUserId;
        this.userCreditLookupTableModal.show();
    }


        setUserCreditIdNull() {
        this.creditHistory.userCreditId = null;
        this.userCreditUserId = '';
    }


        getNewUserCreditId() {
        this.creditHistory.userCreditId = this.userCreditLookupTableModal.id;
        this.userCreditUserId = this.userCreditLookupTableModal.displayName;
    }


    close(): void {

        this.active = false;
        this.modal.hide();
    }
}

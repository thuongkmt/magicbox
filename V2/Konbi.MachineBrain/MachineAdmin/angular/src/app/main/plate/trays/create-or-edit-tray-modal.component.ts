import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { TrayServiceProxy, TrayDto } from '@shared/service-proxies/tray-service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';


@Component({
    selector: 'createOrEditTrayModal',
    templateUrl: './create-or-edit-tray-modal.component.html'
})
export class CreateOrEditTrayModalComponent extends AppComponentBase {

    @ViewChild('createOrEditTrayModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    tray: TrayDto = new TrayDto();

    constructor(
        injector: Injector,
        private _trayServiceProxy: TrayServiceProxy
    ) {
        super(injector);
    }

    show(tray?: TrayDto): void {
        if (tray == null) {
            this.tray = new TrayDto();
        }
        else {
            this.tray = tray;
        }
        this.active = true;
        this.modal.show();
    }

    save(): void {
        this.primengTableHelper.showLoadingIndicator();
        this.saving = true;
        this._trayServiceProxy.createOrEditTray(this.tray)
            .pipe(finalize(() => { this.saving = false; }))
            .subscribe((result) => {
                if(result.message != null)
                {
                    abp.message.error(result.message);
                }
                else
                {
                    this.close();
                    this.modalSave.emit(null);
                    this.notify.info(this.l('SavedSuccessfully'));
                }
            });
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }
}
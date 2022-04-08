import { Component, ViewChild, Injector, Output, EventEmitter, NgZone } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { PlateMenuServiceProxy, PlateMenuDto, PlateMenuInputDto, ImportResult, ReplicateInput } from '@shared/service-proxies/plate-menu-service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ngfModule, ngf } from "angular-file"

@Component({
  selector: 'replicateModal',
  templateUrl: './replicate.component.html',
  styleUrls: ['./replicate.component.css']
})
export class ReplicateComponent extends AppComponentBase {

    @ViewChild('replicateModal') modal: ModalDirective;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    selectedDate: Date;
    days: number = 7;

    constructor(
        injector: Injector,
        private _plateMenusServiceProxy: PlateMenuServiceProxy
    ) {
        super(injector);
    }

    show(selectedDate: Date): void {
        this.selectedDate = selectedDate;
        this.active = true;
        this.modal.show();
    }

    save(): void {
        this.saving = true;
        let input = new ReplicateInput();
        input.dateFilter = this.selectedDate;
        input.days = this.days;
        this._plateMenusServiceProxy.replicateData(input)
        .subscribe((result) => {
            this.saving = false;
            abp.message.success('Replicate Plate Menu successfull!');
        });
    }

    close(): void {
        this.days = 7;
        this.active = false;
        this.saving = false;
        this.modal.hide();
    }
}
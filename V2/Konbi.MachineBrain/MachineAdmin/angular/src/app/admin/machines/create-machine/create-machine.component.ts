import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef, OnInit } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';


import {
    ListResultDtoOfPermissionDto
} from 'shared/service-proxies/service-proxies';
import { MachineServiceProxy, CreateMachineDto, ICreateMachineDto } from 'shared/service-proxies/machine-service-proxies';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
    selector: 'create-machine-modal',
    templateUrl: './create-machine.component.html',
    styleUrls: ['./create-machine.component.css']
})
export class CreateMachineComponent extends AppComponentBase implements OnInit {
    @ViewChild('createMachineModal') modal: ModalDirective;
    @ViewChild('modalContent') modalContent: ElementRef;

    active: boolean = false;
    saving: boolean = false;

    permissions: ListResultDtoOfPermissionDto = null;
    machine: CreateMachineDto = null;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
    constructor(
        injector: Injector,
        private _machineService: MachineServiceProxy
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this._machineService.getAllPermissions()
            .subscribe((permissions: ListResultDtoOfPermissionDto) => {
                this.permissions = permissions;
            });
    }

    show(): void {
        this.active = true;        
        this.machine = new CreateMachineDto();
        this.machine.init({ isStatic: false, id: this.guid() });
        this.modal.show();
    }

    guid() {
        return this.s4() + this.s4() + '-' + this.s4() + '-' + this.s4() + '-' +
            this.s4() + '-' + this.s4() + this.s4() + this.s4();
    }

    s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    }

    onShown(): void {
        $.AdminBSB.input.activate($(this.modalContent.nativeElement));
    }

    save(): void {
        this.saving = true;        
        this._machineService.create(this.machine)
            .finally(() => { this.saving = false; })
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

import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef, OnInit } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { ListResultDtoOfPermissionDto } from '@shared/service-proxies/service-proxies';
import { MachineServiceProxy, MachineDto } from '@shared/service-proxies/machine-service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
//declare var $: any;

@Component({
    selector: 'edit-machine-modal',
    templateUrl: './edit-machine.component.html',
    styleUrls: ['./edit-machine.component.css']
})

export class EditMachineComponent extends AppComponentBase implements OnInit {
    @ViewChild('editMachineModal') modal: ModalDirective;
    @ViewChild('modalContent') modalContent: ElementRef;

    active: boolean = false;
    saving: boolean = false;

    permissions: ListResultDtoOfPermissionDto = null;
    machine: MachineDto = null;

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

    show(id: string): void {
        this._machineService.get(id)
            .finally(() => {
                this.active = true;
                this.modal.show();
            })
            .subscribe((result: MachineDto) => {
                this.machine = result;
            });
    }

    onShown(): void {
        //$.AdminBSB.input.activate($(this.modalContent.nativeElement));
    }


    save(): void {
        var permissions = [];
        // $(this.modalContent.nativeElement).find("[name=permission]").each(
        //     function (index: number, elem: Element) {
        //         if ($(elem).is(":checked") == true) {
        //             permissions.push(elem.getAttribute("value").valueOf());
        //         }
        //     }
        // )
        this.saving = true;
        this._machineService.update(this.machine)
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

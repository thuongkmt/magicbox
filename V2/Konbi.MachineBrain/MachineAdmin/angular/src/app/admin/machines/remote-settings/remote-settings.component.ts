import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef, OnInit } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';

import {
    ListResultDtoOfPermissionDto
} from 'shared/service-proxies/service-proxies';

import { MachineServiceProxy, SendRemoteCommandInput, SendRemoteCommandOutputDto } from 'shared/service-proxies/machine-service-proxies';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
    selector: 'remote-settings-machine-modal',
  templateUrl: './remote-settings.component.html',
  styleUrls: ['./remote-settings.component.css']
})

export class RemoteSettingsComponent extends AppComponentBase implements OnInit {
   // @ViewChild('remoteSettingsMachineModal') modal: ModalDirective;
    @ViewChild('modalContent') modalContent: ElementRef;

    active: boolean = false;
    saving: boolean = false;
    machineID: string;
    command: SendRemoteCommandInput = null;
    overleyCustomerMsg: string;
    moveToEvelator: string;
    dispenseSlotNumber: string;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
    constructor(
        injector: Injector,
        private route: ActivatedRoute,
        private location: Location,
        private _machineService: MachineServiceProxy
    ) {
        super(injector);
    }

    ngOnInit(): void {
        //this.machineID = this.route.snapshot.paramMap.get('id');
        //console.log('Machine ID from route = ' + this.machineID);
        this.show(this.route.snapshot.paramMap.get('id'));
    }

    show(id: string): void {
        this.machineID = id;
        this.command = new SendRemoteCommandInput();
        this.command.MachineID = id;
       
       //  console.log('show settings for machine ' + id);
       // this.active = true;
       // this.modal.show();
    }
    
    onShown(): void {
        $.AdminBSB.input.activate($(this.modalContent.nativeElement));
    }

    sendCommand(cmd: string): void {
        console.log('Command = ' + cmd);

        //this.saving = true;
       // console.log(this.command);       
        this.command.CommandName = cmd;
        switch (this.command.CommandName)
        {
            case 'SetOverleyCustomerTemporalMessage':
                this.command.CommandArgs = this.overleyCustomerMsg;
                if (this.overleyCustomerMsg == undefined || this.overleyCustomerMsg == '') {
                    this.notify.info(this.l("Please input message content for customer:"));
                    return;
                }
                break;
            case 'RemoteMoveElevatorToLevel':
                this.command.CommandArgs = this.moveToEvelator;
                if (this.moveToEvelator == undefined || this.moveToEvelator == '') {
                    this.notify.info(this.l("Please input evevator number:"));
                    return;
                }
                break;
            case 'RemoteDispense':
                this.command.CommandArgs = this.dispenseSlotNumber;
                if (this.moveToEvelator == undefined || this.moveToEvelator == '') {
                    this.notify.info(this.l("Please input dispense slot number:"));
                    return;
                }
                break;
        }

        console.log(this.command);

        this._machineService.sendCommandToMachine(this.command)
            .finally(() => { this.saving = false; })
            .subscribe((result) => {
               // console.log(result);
                if (result.IsSuccess) {
                   this.notify.info(this.l('Command Sent to Machine Successfully'));
                  ///  this.close();
                  //  this.modalSave.emit(null);
                }
                else {
                    this.notify.info(this.l("Error:") + " " + result.Message);
                }

            });
    }

    close(): void {
        this.active = false;
     
    }
}

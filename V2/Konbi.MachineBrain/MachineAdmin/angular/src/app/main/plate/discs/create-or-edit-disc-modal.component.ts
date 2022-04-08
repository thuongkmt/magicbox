import { Component, ViewChild, Injector, Output, EventEmitter, NgZone } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
//import { DiscsServiceProxy, CreateOrEditDiscDto, GetDiscForView } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { PlateLookupTableModalComponent } from './plate-lookup-table-modal.component';
import { ISubscription } from "rxjs/Subscription";
//add signalr
import { SignalRHelper } from 'shared/helpers/SignalRHelper';
import { MessageSignalrService } from '../../../shared/common/signalr/message-signalr.service';

import { ParentChildService } from '../../../shared/common/parent-child/parent-child.service'

@Component({
    selector: 'createOrEditDiscModal',
    templateUrl: './create-or-edit-disc-modal.component.html',
    styleUrls: ['./create-or-edit-disc-modal.component.css'],
})
export class CreateOrEditDiscModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @ViewChild('plateLookupTableModal') plateLookupTableModal: PlateLookupTableModalComponent;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    // public listDishAvailabe: Array<GetDiscForView> = new Array<GetDiscForView>();
    // public lstCreateDish: Array<CreateOrEditDiscDto> = new Array<CreateOrEditDiscDto>();

    plateId = '';
    plateCode = '';
    plateName = '';
    isCreateForPlate = false;

    private onDetectDishFromTable: ISubscription

    constructor(
        injector: Injector,
     //   private _discsServiceProxy: DiscsServiceProxy,
        private _messageSignalrService: MessageSignalrService,
        public _zone: NgZone,
        private comParentChildService: ParentChildService
    ) {
        super(injector);
    }

    ngOnInit(): void {
        if (this.appSession.application) {
            if (!this._messageSignalrService.isMessageConnected) {
                SignalRHelper.initSignalR(() => {
                    this._messageSignalrService.init();
                });
            }
        }
        if (!this.onDetectDishFromTable) {
            this.onDetectDishFromTable = this.comParentChildService.on('app.message.messageReceived').subscribe($data => this.onMessageReceived($data));
        }
    }

    ngOnDestroy() {
        if (this.onDetectDishFromTable) this.onDetectDishFromTable.unsubscribe()
    }

    // init(): void {
    //     this.registerEvents();
    // }

    onMessageReceived(message) {
        // let mess = JSON.parse(message.message);

        // if (mess.type = 'RFIDTable_DetectedDisc') {
        //     if (!this.active) return

        //     if (!this.plateId) {
        //         abp.message.error('Please select plate first to start counting !')
        //         return
        //     }

        //     let tmpDishes: Array<CreateOrEditDiscDto> = new Array<CreateOrEditDiscDto>();

        //     for (let dish of mess.data.Plates) {
        //         if (dish.UType != this.plateCode) {
        //             let errMessage = 'The plate code ' + dish.UType + ' is invalid, please choose a plate with code ' + this.plateCode;
        //             abp.message.error(errMessage)
        //             return;
        //         }
        //         if (this.listDishAvailabe.filter(item => item.disc.uid == dish.UID).length > 0
        //             || this.lstCreateDish.filter(item => item.uid == dish.UID).length > 0
        //             || tmpDishes.filter(item => item.uid == dish.UID).length > 0
        //         ) {
        //             let errMessage = 'The plate ID ' + dish.UID + ' is already registered, please use another plate.'
        //             abp.message.error(errMessage)
        //             return;
        //         }

        //         let newDish = new CreateOrEditDiscDto()
        //         newDish.plateId = this.plateId
        //         newDish.code = dish.UType
        //         newDish.uid = dish.UID
        //         tmpDishes.push(newDish);
        //     }

        //     this._zone.run(() => {
        //         this.lstCreateDish = this.lstCreateDish.concat(tmpDishes);
        //     });
        // }
        //abp.utils.formatString('{0}: {1}', 'aaaaaaaaaaaaa', abp.utils.truncateString(message.message, 100));
    };


    // registerEvents(): void {
    //     const self = this;
    //     function onMessageReceived(message) {
    // 		console.log("message json =", message);
    //         let mess = JSON.parse(message.message);

    //         if (mess.type = 'RFIDTable_DetectedDisc') {
    //             if (!self.active) return

    //             if (!self.plateId) {
    //                 abp.message.error('Please select plate first to start counting !')
    //                 return
    //             }

    //             let tmpDishes: Array<CreateOrEditDiscDto> = new Array<CreateOrEditDiscDto>();

    //             for (let dish of mess.data.Plates) {
    //                 if (dish.UType != self.plateCode) {
    //                     let errMessage =  'The plate code ' + dish.UType + ' is invalid, please choose a plate with code ' + self.plateCode;
    //                     abp.message.error(errMessage)
    //                     return;
    //                 }
    //                 if (self.listDishAvailabe.filter(item => item.disc.uid == dish.UID).length > 0
    //                     || self.lstCreateDish.filter(item => item.uid == dish.UID).length > 0
    //                     || tmpDishes.filter(item => item.uid == dish.UID).length > 0
    //                 ) {
    //                     let errMessage = 'The plate ID ' + dish.UID + ' is already registered, please use another plate.'
    //                     abp.message.error(errMessage)
    //                     return;
    //                 }

    //                 let newDish = new CreateOrEditDiscDto()
    //                 newDish.plateId = self.plateId
    //                 newDish.code = dish.UType
    //                 newDish.uid = dish.UID
    //                 tmpDishes.push(newDish);
    //             }
    //             self.lstCreateDish = self.lstCreateDish.concat(tmpDishes);

    //         }
    //         //abp.utils.formatString('{0}: {1}', 'aaaaaaaaaaaaa', abp.utils.truncateString(message.message, 100));
    //     };

    //     abp.event.on('app.message.messageReceived', message => {
    //         self._zone.run(() => {
    //             onMessageReceived(message);
    //         });
    //     });
    // }

    show(plateId?: string, plateCode?: string, plateName?: string): void {
        if (plateId) {
            this.plateId = plateId;
            this.plateCode = plateCode;
            this.plateName = plateName;
            //this.isCreateForPlate = true;
            this.getDishesOfPlate(plateId);
        }
        this.active = true;
        this.modal.show();
    }

    save(): void {
        // this.saving = true;
        // this._discsServiceProxy.createOrEdit(this.lstCreateDish)
        //     .pipe(finalize(() => { this.saving = false; }))
        //     .subscribe(() => {
        //         this.notify.info(this.l('SavedSuccessfully'));
        //         this.close();
        //         this.modalSave.emit(null);
        //     });
    }

    openSelectPlateModal() {
        //this.plateLookupTableModal.id = this.disc.plateId;
        this.plateLookupTableModal.displayName = this.plateName;
        this.plateLookupTableModal.show();
    }

    //reset pick plate
    setPlateIdNull() {
        // this.plateId = null;
        // this.plateId = '';
        // this.plateName = '';
        // this.lstCreateDish = new Array<CreateOrEditDiscDto>();
        // this.listDishAvailabe = new Array<GetDiscForView>();
    }

    //callback from pick plate
    getNewPlateId() {
        this.plateId = this.plateLookupTableModal.id;
        this.plateName = this.plateLookupTableModal.displayName;
        this.plateCode = this.plateLookupTableModal.code;
        this.getDishesOfPlate(this.plateId)
    }

    //get dishes of plate to check available
    getDishesOfPlate(plateId) {
        // this._discsServiceProxy.getAll('', '', '', '', plateId, '', 0, 1000
        // ).subscribe(result => {
        //     this.listDishAvailabe = result.items;
        // });
    }

    //delete dishes item in list create
    deleteDisc(dish) {
        // this.lstCreateDish = this.lstCreateDish.filter(function (obj) {
        //     return obj.uid !== dish.uid;
        // });
    }

    close(): void {
        // this.plateId = null;
        // this.plateId = '';
        // this.plateName = '';
        // this.lstCreateDish = new Array<CreateOrEditDiscDto>();
        // this.listDishAvailabe = new Array<GetDiscForView>();
        // this.active = false;
        // this.modal.hide();
    }
}
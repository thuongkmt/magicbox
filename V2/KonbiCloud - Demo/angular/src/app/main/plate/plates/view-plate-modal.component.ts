// import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
// import { ModalDirective } from 'ngx-bootstrap';
// import {  PlateDto } from '@shared/service-proxies/plate-service-proxies';
// import { AppComponentBase } from '@shared/common/app-component-base';

// @Component({
//     selector: 'viewPlateModal',
//     templateUrl: './view-plate-modal.component.html'
// })
// export class ViewPlateModalComponent extends AppComponentBase {

//     @ViewChild('createOrEditModal') modal: ModalDirective;


//     @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

//     active = false;
//     saving = false;

//     item : GetPlateForView;
	

//     constructor(
//         injector: Injector
//     ) {
//         super(injector);
//         this.item = new GetPlateForView();
//         this.item.plate = new PlateDto();
//     }

//     show(item: GetPlateForView): void {
//         this.item = item;
//         this.active = true;
//         this.modal.show();
//     }
    
//     close(): void {
//         this.active = false;
//         this.modal.hide();
//     }
// }

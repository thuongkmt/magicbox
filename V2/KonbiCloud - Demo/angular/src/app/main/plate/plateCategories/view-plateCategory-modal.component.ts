// import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
// import { ModalDirective } from 'ngx-bootstrap';
// import { PlateCategoryDto } from '@shared/service-proxies/plate-category-service-proxies';
// import { AppComponentBase } from '@shared/common/app-component-base';

// @Component({
//     selector: 'viewPlateCategoryModal',
//     templateUrl: './view-plateCategory-modal.component.html'
// })
// export class ViewPlateCategoryModalComponent extends AppComponentBase {

//     @ViewChild('createOrEditModal') modal: ModalDirective;


//     @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

//     active = false;
//     saving = false;

//     // item : GetPlateCategoryForView;
	

//     constructor(
//         injector: Injector
//     ) {
//         super(injector);
//         // this.item = new GetPlateCategoryForView();
//         // this.item.plateCategory = new PlateCategoryDto();
//     }

//     // show(item: GetPlateCategoryForView): void {
//     //     this.item = item;
//     //     this.active = true;
//     //     this.modal.show();
//     // }
    
//     close(): void {
//         this.active = false;
//         this.modal.hide();
//     }
// }

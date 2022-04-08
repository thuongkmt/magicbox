import { Component, ViewChild, Injector, Output, EventEmitter} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { PlateCategoryServiceProxy, CreatePlateCategoryDto, CreateOrEditPlateCategoryDto } from '@shared/service-proxies/plate-category-service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';


@Component({
    selector: 'createOrEditPlateCategoryModal',
    templateUrl: './create-or-edit-plateCategory-modal.component.html'
})
export class CreateOrEditPlateCategoryModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    plateCategory: CreateOrEditPlateCategoryDto = new CreateOrEditPlateCategoryDto();
	
    constructor(
        injector: Injector,
        private _plateCategoriesServiceProxy: PlateCategoryServiceProxy
    ) {
        super(injector);
    }

    show(plateCategoryId?: number): void {
        if (!plateCategoryId) { 
			this.plateCategory = new CreateOrEditPlateCategoryDto();
			// this.plateCategory.id = plateCategoryId;
			this.active = true;
			this.modal.show();
        }
		else{
			this._plateCategoriesServiceProxy.get(plateCategoryId).subscribe(result => {
				this.plateCategory = result.plateCategory;
				this.active = true;
				this.modal.show();
			});
		}  
    }

    save(): void {
			this.saving = true;
			this._plateCategoriesServiceProxy.createOrUpdate(this.plateCategory)
			 .pipe(finalize(() => { this.saving = false; }))
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
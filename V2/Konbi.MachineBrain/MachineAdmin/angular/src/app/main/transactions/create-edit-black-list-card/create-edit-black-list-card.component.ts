import { Component, OnInit, ViewChild, Output,EventEmitter, Injector } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { AppComponentBase } from '@shared/common/app-component-base';
import { BlackListCardServiceProxiesService, BlackListCardDto } from '@shared/service-proxies/black-list-card-service-proxies.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'createOrEditBlackListCardModal',
  templateUrl: './create-edit-black-list-card.component.html',
  styleUrls: ['./create-edit-black-list-card.component.css']
})
export class CreateEditBlackListCardComponent extends AppComponentBase {

  @ViewChild('createOrEditModal') modal: ModalDirective;

  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
  blackListCard: BlackListCardDto = new BlackListCardDto();
  active = false;
  saving = false;

  constructor(injector: Injector,
    private _blackListCardServiceProxy: BlackListCardServiceProxiesService) {
      super(injector);
     }

  ngOnInit() {
  }

  show(id?: string): void {
    if (!id) {
        this.blackListCard = new BlackListCardDto();
        this.blackListCard.id = id;
        this.active = true;
        this.modal.show();
    } else {
        this._blackListCardServiceProxy.getBlackListCardForEdit(id).subscribe(result => {
            this.blackListCard = result;
            this.active = true;
            this.modal.show();
        });
    }
  }

  save(): void {
          this.saving = true;
          this._blackListCardServiceProxy.createOrEdit(this.blackListCard)
          .pipe(finalize(() => { this.saving = false;}))
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

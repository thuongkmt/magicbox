import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { AppComponentBase } from '@shared/common/app-component-base';
import { HttpClient } from '@angular/common/http';
import { AppConsts } from '@shared/AppConsts';

@Component({
  selector: 'editSettingModal',
  templateUrl: './edit-setting.component.html'
})
export class EditSettingComponent extends AppComponentBase {

  private httpClient: HttpClient;
  private url: string;

  constructor(
    injector: Injector,
    private http: HttpClient
  ) {
    super(injector);
    this.httpClient = http;
    this.url = AppConsts.remoteServiceBaseUrl;
  }

  @ViewChild('editSettingModal') modal: ModalDirective;
  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();


  active = false;
  saving = false;

  show() {
    this.active = true;
    this.modal.show();
  }

  close(): void {
    this.active = false;
    this.modal.hide();
  }

}

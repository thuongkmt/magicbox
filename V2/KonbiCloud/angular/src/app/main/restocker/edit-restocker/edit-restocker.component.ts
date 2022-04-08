import { Component, OnInit, ViewChild, Output, EventEmitter, Injector } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ModalDirective } from 'ngx-bootstrap';
import { UserServiceProxy, PasswordComplexitySetting, ProfileServiceProxy, UserEditDto } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-edit-restocker',
  templateUrl: './edit-restocker.component.html',
  styleUrls: ['./edit-restocker.component.css']
})

export class EditRestockerComponent extends AppComponentBase {

  @ViewChild('editRestockerModal') modal: ModalDirective;
  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
  passwordComplexitySetting: PasswordComplexitySetting = new PasswordComplexitySetting();
  passwordComplexityInfo = '';
  restocker : UserEditDto;
  active = false;
  saving = false;

  constructor(injector: Injector,private _userServiceProxy : UserServiceProxy,private _profileService : ProfileServiceProxy) {
      super(injector);
  }

  show(id?: number): void {
    if(id){
      this._userServiceProxy.getRestockerForEdit(id).subscribe(result => {
        this.restocker = result;
        
        this._profileService.getPasswordComplexitySetting().subscribe(passwordComplexityResult => {
          this.passwordComplexitySetting = passwordComplexityResult.setting;
          this.setPasswordComplexityInfo();
          this.active = true;
          this.modal.show();
        });
      });
    }
    else{
        this.restocker = new UserEditDto();
        this.active = true;
        this.modal.show();
    }
  }

  setPasswordComplexityInfo(): void {

    this.passwordComplexityInfo = '<ul>';

    if (this.passwordComplexitySetting.requireDigit) {
        this.passwordComplexityInfo += '<li>' + this.l('PasswordComplexity_RequireDigit_Hint') + '</li>';
    }

    if (this.passwordComplexitySetting.requireLowercase) {
        this.passwordComplexityInfo += '<li>' + this.l('PasswordComplexity_RequireLowercase_Hint') + '</li>';
    }

    if (this.passwordComplexitySetting.requireUppercase) {
        this.passwordComplexityInfo += '<li>' + this.l('PasswordComplexity_RequireUppercase_Hint') + '</li>';
    }

    if (this.passwordComplexitySetting.requireNonAlphanumeric) {
        this.passwordComplexityInfo += '<li>' + this.l('PasswordComplexity_RequireNonAlphanumeric_Hint') + '</li>';
    }

    if (this.passwordComplexitySetting.requiredLength) {
        this.passwordComplexityInfo += '<li>' + this.l('PasswordComplexity_RequiredLength_Hint', this.passwordComplexitySetting.requiredLength) + '</li>';
    }

    this.passwordComplexityInfo += '</ul>';
  }

  save(): void {
    this.saving = true;
    this._userServiceProxy.createOrEditRestocker(this.restocker)
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

  ngOnInit() {
  }

}

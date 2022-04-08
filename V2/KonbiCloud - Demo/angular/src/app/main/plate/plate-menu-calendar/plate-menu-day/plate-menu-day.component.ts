import { Component, ViewChild, Injector, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { PlateMenuServiceProxy, PlateMenuDayResult } from '@shared/service-proxies/plate-menu-service-proxies';
import { Router } from '@angular/router';

@Component({
  selector: 'app-plate-menu-day',
  templateUrl: './plate-menu-day.component.html',
  styleUrls: ['./plate-menu-day.component.css']
})
export class PlateMenuDayComponent extends AppComponentBase {

  @ViewChild('plateMenuDay') modal: ModalDirective;

  seletedDate: string
  seletedDateFriendly: string

  lstPlateMenuDetail: PlateMenuDayResult[] = []
  isLoading = false

  constructor(
    injector: Injector,
    private _plateMenusServiceProxy: PlateMenuServiceProxy,
    private changeDetectorRef: ChangeDetectorRef,
    private router: Router,
  ) {
    super(injector);
  }

  show(date: Date): void {
    //this.seletedDate = date.toString()

    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    var day = ("0" + date.getDate()).slice(-2);
    this.seletedDate = month + "/" + day + "/" + date.getFullYear()
    this.seletedDateFriendly = day + "/" + month + "/" + date.getFullYear()
    this.getDayPlateDetail(this.seletedDate)
  }

  getDayPlateDetail(date: String) {
    this.isLoading = true
    this._plateMenusServiceProxy.GetPlateMenuByDay(date).subscribe(result => {
      if (result != null) {
        this.lstPlateMenuDetail = result
      }
      this.isLoading = false
      this.modal.show();
      this.changeDetectorRef.detectChanges();
    });
  }

  onViewMenuClick(item: PlateMenuDayResult) {
    let items = this.seletedDateFriendly.split("/");
    let routerUrl = "app/main/plate/plateMenus/" + items[2] + "-" + items[1] + "-" + items[0] + "/" + item.sessionId
    this.router.navigate([routerUrl]);
  }

  btnSetMenuClick(){
    let items = this.seletedDateFriendly.split("/");
    let routerUrl = "app/main/plate/plateMenus/" + items[2] + "-" + items[1] + "-" + items[0]
    this.router.navigate([routerUrl]);
  }

  close(): void {
    this.lstPlateMenuDetail = []
    this.modal.hide();
  }

}
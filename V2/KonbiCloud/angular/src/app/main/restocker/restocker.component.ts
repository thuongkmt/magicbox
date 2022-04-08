import { Component, OnInit, Injector, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { Table } from 'primeng/table';
import { Paginator, LazyLoadEvent } from 'primeng/primeng';
import { UserServiceProxy } from '@shared/service-proxies/service-proxies';
import { EditRestockerComponent } from './edit-restocker/edit-restocker.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';

@Component({
  selector: 'app-restocker',
  templateUrl: './restocker.component.html',
  styleUrls: ['./restocker.component.css'],
  animations: [appModuleAnimation()]
})
export class RestockerComponent extends AppComponentBase {

  @ViewChild('dataTable') dataTable: Table;
  @ViewChild('paginator') paginator: Paginator;
  @ViewChild('editRestockerModal') editRestockerModal: EditRestockerComponent;
  filterText : string;
  
  constructor(injector: Injector,
    private _userServiceProxy: UserServiceProxy,) { 
    super(injector);
  }

  getRestockers(event?: LazyLoadEvent){
    if (this.primengTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);
      return;
  }

  this.primengTableHelper.showLoadingIndicator();

  this._userServiceProxy.getRestockers(
      this.filterText,
      this.primengTableHelper.getSorting(this.dataTable),
      this.primengTableHelper.getSkipCount(this.paginator, event),
      this.primengTableHelper.getMaxResultCount(this.paginator, event)
      ).subscribe(result => {
          this.primengTableHelper.totalRecordsCount = result.totalCount;
          this.primengTableHelper.records = result.items;
          this.primengTableHelper.hideLoadingIndicator();
      });
  }

  createRestocker(): void {
    this.editRestockerModal.show();
  }

  deleteRestocker(id: number,name: string): void {
    this.message.confirm(
        this.l('UserDeleteWarningMessage', name),
        this.l('AreYouSure'),
        (isConfirmed) => {
            if (isConfirmed) {
                this._userServiceProxy.deleteUser(id)
                    .subscribe(() => {
                        this.reloadPage();
                        this.notify.success(this.l('SuccessfullyDeleted'));
                    });
            }
        }
    );
  }

  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
  }

  ngOnInit() {
  }

}

import { Component, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import * as _ from 'lodash';
import { TrayServiceProxy, TrayDto } from '@shared/service-proxies/tray-service-proxies';
import { Http } from '@angular/http';
import { NotifyService } from '@abp/notify/notify.service';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import * as moment from 'moment';
import { CreateOrEditTrayModalComponent } from './create-or-edit-tray-modal.component';

@Component({
  templateUrl: './trays.component.html',
  styleUrls: ['./trays.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})
export class TraysComponent extends AppComponentBase {

  @ViewChild('transactionTable') transactionTable: Table;
  @ViewChild('paginator') paginator: Paginator;
  @ViewChild('createOrEditTrayModal') createOrEditTrayModal: CreateOrEditTrayModalComponent;

  nameFilter;
  codeFilter = '';

  constructor(
    injector: Injector,
    private route: ActivatedRoute,     
    private _trayService: TrayServiceProxy
  ) {
      super(injector);
  }

    reloadPage(): void {
      this.paginator.changePage(this.paginator.getPage());
    }

    getTrays(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
          this.paginator.changePage(0);
          return;
      }

      this.primengTableHelper.showLoadingIndicator();
      this._trayService.getAllTrays(
        this.nameFilter,
        this.codeFilter,
        this.primengTableHelper.getSorting(this.transactionTable),
        1000,
        0
        // this.primengTableHelper.getMaxResultCount(this.paginator, event),
        // this.primengTableHelper.getSkipCount(this.paginator, event)
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    deleteTray(id: string): void {
        this.message.confirm(
            'Are you sure you want to delete tray?',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._trayService.deleteTray(id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }

    syncTray()
    {
        this.primengTableHelper.showLoadingIndicator();
        this._trayService.syncTray()
        .finally(() => {
            this.primengTableHelper.hideLoadingIndicator();
        })
        .subscribe(() => {
            this.reloadPage();
            this.primengTableHelper.hideLoadingIndicator();
            this.notify.success('Data synced');
        });
    }
}


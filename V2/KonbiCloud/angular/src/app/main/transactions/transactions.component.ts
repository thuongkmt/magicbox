import { Component, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import * as _ from 'lodash';
import { TransactionServiceProxy, TransactionDto } from '@shared/service-proxies/transaction-service-proxies';
import { Http } from '@angular/http';
import { NotifyService } from '@abp/notify/notify.service';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import * as moment from 'moment';
import { TransactionDetailComponent } from './transaction-detail/transaction-detail.component';
import { SessionsServiceProxy, GetSessionForView } from '@shared/service-proxies/service-proxies';
import { MachineServiceProxy, MachineDto, PagedResultDtoOfMachineDto } from "shared/service-proxies/machine-service-proxies";

@Component({
  templateUrl: './transactions.component.html',
  styleUrls: ['./transactions.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})
export class TransactionsComponent extends AppComponentBase {

  @ViewChild('transactionTable') transactionTable: Table;
  @ViewChild('paginator') paginator: Paginator;
  @ViewChild('transactionDetailModal') transactionDetailComponent: TransactionDetailComponent;

  fromDate = '';
  toDate = '';
  machineFilter = '';
  sessionFilter = '';
  stateFilter = '';
  currencySymbol = '$';
  sessions : GetSessionForView[] = [];
  transType = 1;
  machines: MachineDto[] = [];

  constructor(
    injector: Injector,
    private route: ActivatedRoute,
    private router: Router,
    private _transactionService: TransactionServiceProxy,
    private _sessionService : SessionsServiceProxy,
    private machinesService: MachineServiceProxy
  ) {
      super(injector);

      if(this.router.url.includes("success")) this.transType = 1
      else  this.transType = 0
  }

    ngOnInit() {
      this.primengTableHelper.showLoadingIndicator();
      this.getSessions();
      this.getMachines();
      this.currencySymbol = abp.setting.get("CurrencySymbol");
    }

    reloadPage(): void {
      this.paginator.changePage(this.paginator.getPage());
    }

    getSessions()
    {
      this._sessionService.getAll(undefined, undefined, undefined, undefined, 'session.name desc', 0, 100)
			.subscribe(result => {
				this.sessions = result.items;
			});
    }

    getMachines(event?: LazyLoadEvent) {
      this.machinesService.getAll(0, 100, 'name asc')
        .subscribe(result => {
          this.machines = result.items;
      });
  }

    getTransactions(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
          this.paginator.changePage(0);
          return;
      }

      this.primengTableHelper.showLoadingIndicator();
      this._transactionService.getAllTransactions(
        undefined,
        this.stateFilter,
        this.transType,
        this.fromDate,
        this.toDate,
        this.machineFilter,
        undefined,
        this.primengTableHelper.getSorting(this.transactionTable),
        this.primengTableHelper.getMaxResultCount(this.paginator, event),
        this.primengTableHelper.getSkipCount(this.paginator, event)
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }
    getDetail(record: TransactionDto): void {
        this.transactionDetailComponent.show(record);
    }
}

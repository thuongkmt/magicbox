import { Component, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Params, Router, NavigationEnd } from '@angular/router';
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
//import { SessionsServiceProxy, GetSessionForView } from '@shared/service-proxies/service-proxies';

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
  sessionFilter = '';
  stateFilter = '';
  currencySymbol = '$';
  //sessions: GetSessionForView[] = [];
  transType = 1;

  constructor(
    injector: Injector,
    private router: Router,
    private _transactionService: TransactionServiceProxy,
    //private _sessionService: SessionsServiceProxy,
  ) {
    super(injector);

    if (this.router.url.includes("success")) this.transType = 1
    else this.transType = 0
  }

  ngOnInit() {
    this.primengTableHelper.showLoadingIndicator();
   // this.getSessions();
    this.currencySymbol = abp.setting.get("CurrencySymbol");
  }

  reloadPage(): void {
    this.paginator.changePage(0);
  }ng 

  // getSessions() {
  //   this._sessionService.getAll(undefined, undefined, undefined, undefined, undefined, 0, 100)
  //     .subscribe(result => {
  //       this.sessions = result.items;
  //     });
  // }

  getTransactions(event?: LazyLoadEvent) {
    if (this.primengTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);
      return;
    }

    this.primengTableHelper.showLoadingIndicator();
    this._transactionService.getAllTransactions(
      this.fromDate,
      this.toDate,
      this.sessionFilter,
      this.stateFilter,
      this.transType,
      this.primengTableHelper.getSorting(this.transactionTable),
      this.primengTableHelper.getMaxResultCount(this.paginator, event),
      this.primengTableHelper.getSkipCount(this.paginator, event)
    ).subscribe(result => {
      this.primengTableHelper.totalRecordsCount = result.totalCount;
      this.primengTableHelper.records = result.items;
      console.log('============================================');
      console.log(result.items);
      this.primengTableHelper.hideLoadingIndicator();
    });
  }
  getDetail(record: TransactionDto): void {
    this.transactionDetailComponent.show(record);
  }

  // Export list transactions to csv.
  exportToCsv(): void {
    // Check exists data export.
    if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
      // Show icon loading.
      this.primengTableHelper.showLoadingIndicator();
      // Load transactions.
      this._transactionService.getAllTransactions(
        this.fromDate,
        this.toDate,
        this.sessionFilter,
        this.stateFilter,
        this.transType,
        this.primengTableHelper.getSorting(this.transactionTable),
        99999,
        0
      ).subscribe(result => {
        if (result.items != null) {
          // Declare csv data.
          let csvData = new Array();
          // Declare const header.
          const header = {
              TransactionId: 'Transaction ID',
              Time: 'Time',
              Amount: 'Amount',
              PaidAmount: 'Paid Amount',
              CardLabel: 'Card Label',
              CardNumber: 'Card Number',
              Products: 'Products',
              States: 'States'
          };
          // Add header to csv data.
          csvData.push(header);
          // add content file csv.
          for (let record of result.items) {
            let products = "";
            if(record.inventories != null && record.inventories.length > 0)
            {
                for(var i= 0; i < record.inventories.length; i++)
                {
                  if( record.inventories[i].product.name != null){
                    products += record.inventories[i].product.name;
                  }
                  
                  if(i != (record.inventories.length -1)){
                    products += ";";
                  }
                }
            }

            csvData.push({
              TransactionId: (record.transactionId == null) ? '' : record.transactionId,
              Time: (record.paymentTime == null) ? '' : record.paymentTime,
              Amount: (record.amount == null) ? '' : record.amount,
              PaidAmount: (record.paidAmount == null) ? '' : record.paidAmount,
              CardLabel: (record.cardLabel == null) ? '' : record.cardLabel,
              CardNumber: (record.cardNumber == null) ? '' : record.cardNumber,
              Products: products,
              States: (record.states == null) ? '' : record.states
            });
          }
          // Export csv data.
          // tslint:disable-next-line:no-unused-expression
          new Angular5Csv(csvData, 'MagicboxTransactions',{quoteStrings: ''});
          // tslint:enable-next-line:no-unused-expression
        } else {
          this.notify.info(this.l('Data is Empty'));
        }
        this.primengTableHelper.hideLoadingIndicator();
      });
    } else {
      this.notify.info(this.l('Data is Empty'));
    }
  }
}

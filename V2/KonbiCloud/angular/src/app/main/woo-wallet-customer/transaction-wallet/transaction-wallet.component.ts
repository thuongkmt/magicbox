import { Component, OnInit, Injector, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ModalDirective } from 'ngx-bootstrap';
import { CustomerWallet, CustomerKonbiWalletServiceServiceProxy } from '@shared/service-proxies/customers-wallet-service-proxies';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';

@Component({
  selector: 'app-transaction-wallet',
  templateUrl: './transaction-wallet.component.html',
  styleUrls: ['./transaction-wallet.component.css']
})
export class TransactionWalletComponent extends AppComponentBase {

  @ViewChild('dataTable') dataTable: Table;
  @ViewChild('paginator') paginator: Paginator;
  @ViewChild('transactionWalletModal') modal: ModalDirective;

  active = false;
  customer: CustomerWallet;

  constructor(
    injector: Injector,
    private _customerKonbiWalletServiceProxy: CustomerKonbiWalletServiceServiceProxy
  ) {
    super(injector);
    // Remove PerPage Woo not support.
    this.primengTableHelper.predefinedRecordsCountPerPage = this.primengTableHelper.predefinedRecordsCountPerPage.filter(x => x <= 100);
  }

  show(record: CustomerWallet): void {
    this.customer = record;
    this.getOrdersByCustomer();
    this.active = true;
    this.modal.show();
  }

  close(): void {
    this.active = false;
    this.modal.hide();
  }

  getOrdersByCustomer(event?: LazyLoadEvent) {
    if (typeof this.customer === typeof undefined) {
      return;
    }

    if (this.primengTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);
      return;
    }

    this.primengTableHelper.showLoadingIndicator();

    // Get Transaction form WooWallet.
    this._customerKonbiWalletServiceProxy.getOrdersByCustomer(
      this.customer.id,
      this.primengTableHelper.getSorting(this.dataTable),
      this.primengTableHelper.getSkipCount(this.paginator, event),
      this.primengTableHelper.getMaxResultCount(this.paginator, event)
    ).subscribe(result => {
      this.primengTableHelper.totalRecordsCount = result.totalCount;
      this.primengTableHelper.records = result.items;
      this.primengTableHelper.hideLoadingIndicator();
    });
  }
}

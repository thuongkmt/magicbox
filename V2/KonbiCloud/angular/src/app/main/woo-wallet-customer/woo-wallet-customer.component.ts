import { Component, Injector, ViewChild, ViewEncapsulation } from '@angular/core';
import { Paginator } from 'primeng/components/paginator/paginator';
import { Table } from 'primeng/components/table/table';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { AppComponentBase } from '@shared/common/app-component-base';
import { CustomerKonbiWalletServiceServiceProxy } from '@shared/service-proxies/customers-wallet-service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { TransactionWalletComponent } from './transaction-wallet/transaction-wallet.component';

@Component({
  selector: 'app-woo-wallet-customer',
  templateUrl: './woo-wallet-customer.component.html',
  styleUrls: ['./woo-wallet-customer.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})
export class WooWalletCustomerComponent extends AppComponentBase {

  @ViewChild('dataTable') dataTable: Table;
  @ViewChild('paginator') paginator: Paginator;
  @ViewChild('transactionWalletModal') transactionWalletModal: TransactionWalletComponent;

  filterText = '';
  customerFilter = '';
  userNameFilter = '';
  emailFilter = '';
  advancedFiltersAreShown = false;

  constructor(
    injector: Injector,
    private _customerKonbiWalletServiceProxy: CustomerKonbiWalletServiceServiceProxy
  ) {
    super(injector);
    // Remove PerPage Woo not support.
    this.primengTableHelper.predefinedRecordsCountPerPage = this.primengTableHelper.predefinedRecordsCountPerPage.filter(x => x <= 100);
  }

  getCustomers(event?: LazyLoadEvent) {
    if (this.primengTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(this.paginator.getPage());
      return;
    }

    this.primengTableHelper.showLoadingIndicator();

    this._customerKonbiWalletServiceProxy.getAll(
      this.filterText,
      this.customerFilter,
      this.userNameFilter,
      this.emailFilter,
      this.primengTableHelper.getSorting(this.dataTable),
      this.primengTableHelper.getSkipCount(this.paginator, event),
      this.primengTableHelper.getMaxResultCount(this.paginator, event)
    ).subscribe(result => {
      this.primengTableHelper.totalRecordsCount = result.totalCount;
      this.primengTableHelper.records = result.items;
      this.primengTableHelper.hideLoadingIndicator();
    });
  }

  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
  }

}

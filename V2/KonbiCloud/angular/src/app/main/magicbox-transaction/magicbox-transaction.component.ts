import { Component, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import * as _ from 'lodash';
import { } from '@shared/service-proxies/transaction-service-proxies';
import { Http } from '@angular/http';
import { NotifyService } from '@abp/notify/notify.service';
import { TokenAuthServiceProxy, TransactionServiceProxy, TransactionDto } from '@shared/service-proxies/service-proxies';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import * as moment from 'moment';
import { MtDetailComponent } from './mt-detail/mt-detail.component';
import { MachineServiceProxy, MachineDto, PagedResultDtoOfMachineDto } from "shared/service-proxies/machine-service-proxies";

@Component({
  templateUrl: './magicbox-transaction.component.html',
  styleUrls: ['./magicbox-transaction.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})
export class MagicboxTransactionComponent extends AppComponentBase {
  @ViewChild('transactionTable') transactionTable: Table;
  @ViewChild('paginator') paginator: Paginator;
  @ViewChild('magixboxTransactionDetailModal') magixboxTransactionDetailModal: MtDetailComponent;

  fromDate = '';
  toDate = '';
  machineFilter = '';
  stateFilter = '';
  currencySymbol = '$';
  transType = 1;
  cardLabelFilter = '';

  machines: MachineDto[] = [];


  constructor(
    injector: Injector,
    private route: ActivatedRoute,
    private router: Router,
    private _transactionService: TransactionServiceProxy,
    private machinesService: MachineServiceProxy
  ) {
    super(injector);

    if (this.router.url.includes("success")) this.transType = 1
    else this.transType = 0
  }

  ngOnInit() {
    this.primengTableHelper.showLoadingIndicator();
    this.getMachines();
    this.currencySymbol = abp.setting.get("CurrencySymbol");
  }

  reloadPage(): void {
    this.paginator.changePage(0);
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
      this.fromDate ? moment(this.fromDate) : undefined,
      this.toDate ? moment(this.toDate) : undefined,
      this.machineFilter,
      this.cardLabelFilter,
      this.primengTableHelper.getSorting(this.transactionTable),
      this.primengTableHelper.getSkipCount(this.paginator, event),
      this.primengTableHelper.getMaxResultCount(this.paginator, event)
    ).subscribe(result => {
      this.primengTableHelper.totalRecordsCount = result.totalCount;
      this.primengTableHelper.records = result.items;
      this.primengTableHelper.hideLoadingIndicator();
    });
  }
  getDetail(record: TransactionDto): void {
    this.magixboxTransactionDetailModal.show(record);
  }

  // Export list transactions to csv.
  exportToCsv(): void {
    // Check exists data export.
    if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
      // Show icon loading.
      this.primengTableHelper.showLoadingIndicator();
      // Load transactions.
      this._transactionService.getAllTransactions(
        undefined,
        this.stateFilter,
        this.transType,
        this.fromDate ? moment(this.fromDate) : undefined,
        this.toDate ? moment(this.toDate) : undefined,
        this.machineFilter,
        this.cardLabelFilter,
        this.primengTableHelper.getSorting(this.transactionTable),
        0,
        9999
      ).subscribe(result => {
        if (result.items != null) {
          // Declare csv data.
          let csvData = new Array();
          // Declare const header.
          const header = {
            TransactionId: 'Transaction ID',
            Machine: 'Machine',
            PaymentTime: 'Payment Time',
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
            if (record.products != null && record.products.length > 0) {
              for (var i = 0; i < record.products.length; i++) {
                if (record.products[i].product.name != null) {
                  products += record.products[i].product.name;
                }

                if (i != (record.products.length - 1)) {
                  products += ";";
                }
              }
            }

            csvData.push({
              TransactionId: (record.transactionId == null) ? '' : record.transactionId,
              Machine: (record.machine == null) ? '' : record.machine,
              PaymentTime: (record.paymentTime == null) ? '' : record.paymentTime,
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
          new Angular5Csv(csvData, 'MagicboxTransactions', { quoteStrings: '' });
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
  // Export list transactions to csv.exportTnxItemsCvs
  exportFinanceCsv(): void {
    // Check exists data export.
    if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
      // Show icon loading.
      this.primengTableHelper.showLoadingIndicator();
      // Load transactions.
      this._transactionService.getAllTransactionFinanceReport(
        undefined,
        undefined,
        this.transType,
        this.fromDate ? moment(this.fromDate) : undefined,
        this.toDate ? moment(this.toDate) : undefined,
        this.machineFilter, this.cardLabelFilter, undefined, 0, 1000).subscribe(result => {
        if (result.items != null) {
          // Declare csv data.
          let csvData = new Array();
          // Declare const header.
          const header = {
            MachineId: 'Machine ID',
            TransactionId: 'Transaction ID',
            TransactionDatetime: 'Transaction Date & Time',
            Quantity: 'Quantity',
            TransactionStatus: 'TransactionStatus',
            PaymentType: 'Payment Type',
            CardId: 'Card ID',
            AmountPaid: 'Amount Paid'
          };
          // Add header to csv data.
          csvData.push(header);
          // add content file csv.
          for (let record of result.items) {
            csvData.push({
              MachineId: (record.machine == null) ? '' : record.machine,
              TransactionId: (record.transactionId == null) ? '' : record.transactionId,
              TransactionDatetime: (record.dateTime == null) ? '' : moment(record.dateTime).format('DD/MM/YYYY HH:mm'),
              Quantity: (record.quantity == null) ? '' : record.quantity,
              TransactionStatus: (record.transactionStatus == null) ? '' : record.transactionStatus,
              PaymentType: (record.paymentType == null) ? '' : record.paymentType,
              CardId: (record.cardId == null) ? '' : record.cardId,
              AmountPaid: (record.amountPaid == null) ? '' : record.amountPaid
            });
          }
          // Export csv data.
          // tslint:disable-next-line:no-unused-expression
          new Angular5Csv(csvData, 'MagicboxFinanceTransactions', { quoteStrings: '' });
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
  // Export list transactions to csv.exportTnxItemsCvs
  exportTnxItemsCvs(): void {
    // Check exists data export.
    if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
      // Show icon loading.
      this.primengTableHelper.showLoadingIndicator();
      // Load transactions.
      this._transactionService.getAllTransactionItemsReport(
        undefined,
        undefined,
        this.transType,
        this.fromDate ? moment(this.fromDate) : undefined,
        this.toDate ? moment(this.toDate) : undefined,
        this.machineFilter, this.cardLabelFilter, undefined, 0, 1000

      ).subscribe(result => {
        if (result.items != null) {
          // Declare csv data.
          let csvData = new Array();
          // Declare const header.
          const header = {
            MachineId: 'Machine ID',
            TransactionId: 'Transaction ID',
            TransactionDatetime: 'Transaction Date & Time',
            Category: 'Category',
            ProductName: 'Product Name',
            ExpireDate: 'Expire Date',
            SKU: 'SKU',
            TagId: 'Tag ID',
            ProductUnitPrice: 'Product Unit Price',
            ProductDiscountPrice: 'Product Discount Price',
            TransactionStatus: 'TransactionStatus',
            PaymentType: 'Payment Type',
            CardId: 'Card ID',
            AmountPaid: 'Amount Paid'
          };
          // Add header to csv data.
          csvData.push(header);
          // add content file csv.
          for (let record of result.items) {
            csvData.push({
              MachineId: (record.machine == null) ? '' : record.machine,
              TransactionId: (record.transactionId == null) ? '' : record.transactionId,
              TransactionDatetime: (record.dateTime == null) ? '' : moment(record.dateTime).format('DD/MM/YYYY HH:mm'),
              Category: (record.category == null) ? '' : record.category,
              ProductName: (record.productName == null) ? '' : record.productName,
              ExpireDate: (record.expireDate == null) ? '' : record.expireDate,
              SKU: (record.sku == null) ? '' : record.sku,
              TagId: (record.tagId == null) ? '' : record.tagId,
              ProductUnitPrice: (record.productUnitPrice == null) ? '' : record.productUnitPrice,
              ProductDiscountPrice: (record.productDiscountPrice == null) ? '' : record.productDiscountPrice,
              TransactionStatus: (record.transactionStatus == null) ? '' : record.transactionStatus,
              PaymentType: (record.paymentType == null) ? '' : record.paymentType,
              CardId: (record.cardId == null) ? '' : record.cardId,
              AmountPaid: (record.amountPaid == null) ? '' : record.amountPaid
            });
          }
          // Export csv data.
          // tslint:disable-next-line:no-unused-expression
          new Angular5Csv(csvData, 'MagicboxTransactionItems', { quoteStrings: '' });
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

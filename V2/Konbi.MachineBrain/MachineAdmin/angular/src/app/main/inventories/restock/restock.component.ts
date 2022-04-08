
import { Component, OnInit, Injector, OnDestroy, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { HttpClient } from '@angular/common/http';
import { RxStompService } from '@stomp/ng2-stompjs';
import { Message } from '@stomp/stompjs';
import { Subscription } from 'rxjs';
import { AppConsts } from '@shared/AppConsts';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { Table } from 'primeng/components/table/table';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { InventoriesServiceProxy, GetCurrentTopupDto, TagsInput, ProductTag }
  from '@shared/service-proxies/inventories-service-proxies';
import { PrimengTableHelper } from 'shared/helpers/PrimengTableHelper';
import { Inventory } from '../topup/topup.component';

@Component({
  templateUrl: './restock.component.html',
  styleUrls: ['./restock.component.css'],
  animations: [appModuleAnimation()]
})
export class RestockComponent extends AppComponentBase implements OnInit, OnDestroy {

  @ViewChild('newItemsTable') newItemsTable: Table;
  @ViewChild('newItemsPaginator') newItemsPaginator: Paginator;

  @ViewChild('productTable') productTable: Table;
  @ViewChild('productPaginator') productPaginator: Paginator;

  public totalTag: number;
  public receivedMessages: string[] = [];
  private topicSubscription: Subscription;
  public newTags: Array<ProductTag> = [];
  public processing: boolean;
  private baseUrl: string;
  public currentTopupInfo: GetCurrentTopupDto = new GetCurrentTopupDto();
  public isTopupProcessing: boolean = false;
  machineId: string = '';
  newItemTableHelper: PrimengTableHelper;
  productTableHelper: PrimengTableHelper;
  productGroups: ProductGroup[] = [];
  allowRestock: boolean = false;
  productTags: Array<ProductTag> = [];
  totalProductCount: string = '';

  constructor(
    injector: Injector,
    private rxStompService: RxStompService,
    private http: HttpClient,
    private _inventoriesServiceProxy: InventoriesServiceProxy) {
    super(injector);
    this.baseUrl = AppConsts.remoteServiceBaseUrl;
    this.totalTag = 0;
    console.log('BASE URL: ' + this.baseUrl);
    this.machineId = this.setting.get('RfidFridgeSetting.Machine.Id');

    this.newItemTableHelper = new PrimengTableHelper();
    this.newItemTableHelper.defaultRecordsCountPerPage = 10;
    this.newItemTableHelper.records = [];

    this.productTableHelper = new PrimengTableHelper();
    this.productTableHelper.defaultRecordsCountPerPage = 10;
    this.productTableHelper.records = [];
  }

  ngOnInit() {
    this.getCurrentTopupInfo();
    this.topicSubscription = this.rxStompService.watch('/topic/tagid').subscribe((message: Message) => {
      this.handleMessage(message);
    });
    this.getTotalProductCount();
  }

  insertInventory() {
    this.processing = true;

    let inventories: Array<Inventory> = [];
    this.productTags.forEach(tag => {
      let data = new Inventory();
      data.tagId = tag.tag;
      data.productId = tag.productId;
      data.price = tag.price;
      inventories.push(data);
    });
    console.log(inventories);
    this.http.post<Inventory>(this.baseUrl + '/api/services/app/Inventories/Restock', inventories).subscribe(result => {
      this.RefreshProduct();
      //this.RefreshCloudInventory();
      this.resetData();
      alert('Restock successful');
      this.getCurrentTopupInfo();

    }, error => {
      alert('Restock failed!');
      console.error(error);
      this.processing = false;
    });


  }

  resetData() {
    this.totalTag = 0;
    this.processing = false;
    this.newItemTableHelper.records = [];
    this.newItemTableHelper.totalRecordsCount = 0;

    this.productTableHelper.records = [];
    this.productTableHelper.totalRecordsCount = 0;
    this.getTotalProductCount();
    this.productGroups = [];
    this.productTags = [];
  }

  getCurrentTopupInfo() {
    this._inventoriesServiceProxy.getCurrentTopupInfo().subscribe(result => {
      this.currentTopupInfo = result;
      console.log('=============Current Top Info============');
      console.log(result);
    });
  }

  handleMessage(message: any) {
    if (!this.processing) {
      var tags = JSON.parse(message.body);
      if (tags.length == 0) return;
      var newTags: ProductTag[] = [];
      tags.forEach(tag => {
        var pt = new ProductTag();
        pt.tag = tag.Id;
        newTags.push(pt);
      });
      this.newTags = newTags;

      var showUpItems = newTags.slice(0, this.newItemTableHelper.defaultRecordsCountPerPage);

      this.newItemTableHelper.records = showUpItems;
      this.newItemTableHelper.totalRecordsCount = newTags.length;
      this.totalTag = newTags.length;
    }
  }
  getProductForTags() {
    this.allowRestock = false;
    if (this.newTags.length == 0) {
      return;
    }
    this.productTableHelper.showLoadingIndicator();
    //Build input from new tags table
    var input = new TagsInput();
    input.machineId = this.machineId;
    input.tags = this.newTags.map(x => x.tag);

    this._inventoriesServiceProxy.GetProductByTag(input)
      .finally(() => this.productTableHelper.hideLoadingIndicator())
      .subscribe(result => {
        this.productTags = result;
        //Set mapped status
        this.newTags.forEach((tag) => {
          if (result.some(x => x.tag == tag.tag)) {
            tag.mapped = true;
          }
        });
        let ptGroup: ProductGroup[] = [];
        var group = result.reduce((a, c) => (a[c.productName] = (a[c.productName] || 0) + 1, a), Object.create(null));
        Object.keys(group).map(function (key) {
          var pg = new ProductGroup();
          pg.ProductName = key;
          pg.Tags = group[key];
          ptGroup.push(pg);
        });
        this.productGroups = ptGroup;
        this.productTableHelper.hideLoadingIndicator();
        this.productTableHelper.records = this.productGroups;
        this.productTableHelper.totalRecordsCount = this.productGroups.length;
        this.getTotalProductCount();
        if (result.length != this.newTags.length) {
          abp.message.warn("Please take out unmapped product!");
          this.allowRestock = false;
        }
        else {
          this.allowRestock = true;
        }
      });
  }

  ngOnDestroy() {
    this.topicSubscription.unsubscribe();
  }

  OpenLock() {
    // var obj = [
    //   // Old item
    //   { Id: "E00403500D1116C6"},
    //   { Id: "E00403500D111A6D"},

    //   // New Items
    //   { Id: "E00403500D112090"},
    //   { Id: "E00403500D110BC0"},
    //   { Id: "E00403500D0E93E8"}
    // ];
    // this.handleMessage({destination: '/topic/tagid', body: JSON.stringify(obj)});

    this.rxStompService.publish({ destination: '/topic/command', body: 'FRID_OPEN' });
  }

  RefreshProduct() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'MACHINE_REFRESHPRD' });
  }

  RefreshCloudInventory() {
    console.log("Refresh cloud");
    this.rxStompService.publish({ destination: '/topic/command', body: 'MACHINE_REFRESHCLOUDINVENT' });
  }

  newItemTablePageChanged(event?: LazyLoadEvent) {
    var skip = this.newItemTableHelper.getSkipCount(this.newItemsPaginator, event);
    var max = this.newItemTableHelper.getMaxResultCount(this.newItemsPaginator, event);
    var showUpItems = this.newTags.slice(skip, skip + Number(max));

    this.newItemTableHelper.records = showUpItems;
  }
  productTablePageChanged(event?: LazyLoadEvent) {
    var skip = this.productTableHelper.getSkipCount(this.productPaginator, event);
    var max = this.productTableHelper.getMaxResultCount(this.productPaginator, event);
    var showUpItems = this.productGroups.slice(skip, skip + Number(max));

    this.productTableHelper.records = showUpItems;
    this.getTotalProductCount();
  }

  getTotalProductCount() {
    this.totalProductCount = 'Total Product: ' + this.productTableHelper.totalRecordsCount;
  }
}

export class ProductGroup {
  ProductName: string;
  Tags: number;
}
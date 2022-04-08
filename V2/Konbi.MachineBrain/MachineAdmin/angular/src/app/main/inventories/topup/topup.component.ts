import { Component, OnInit, Injector, NgZone, OnDestroy } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { HubConnection } from '@aspnet/signalr';
import { SignalRHelper } from 'shared/helpers/SignalRHelper';
import * as $ from 'jquery';
import { debug } from 'util';
import { HttpClient } from '@angular/common/http';
import { RxStompService } from '@stomp/ng2-stompjs';
import { Message } from '@stomp/stompjs';
import { Subscription } from 'rxjs';
import { AppConsts } from '@shared/AppConsts';
import { InventoriesServiceProxy, TopupDto, GetCurrentTopupDto } from '@shared/service-proxies/inventories-service-proxies';

@Component({
  selector: 'app-topup',
  templateUrl: './topup.component.html',
  styleUrls: ['./topup.component.css']
})


export class TopupComponent implements OnInit, OnDestroy {
  public products: Product[] = [];
  public selectedProduct: Product;
  public totalTag: number;
  public receivedMessages: string[] = [];
  private topicSubscription: Subscription;
  private httpClient: HttpClient;
  private url: string;
  public selectedPrice: number;
  public tags: Array<TagId> = [];
  public inventories: Array<Inventory> = [];
  public processing: boolean;
  private baseUrl: string;
  public currentTopup: TopupDto = new TopupDto();
  public currentTopupInfo: GetCurrentTopupDto = new GetCurrentTopupDto();
  public isTopupProcessing: boolean = false;

  constructor(private rxStompService: RxStompService,
    private http: HttpClient,
    private _inventoriesServiceProxy: InventoriesServiceProxy) {
    this.baseUrl = AppConsts.remoteServiceBaseUrl;
    this.totalTag = 0;
    this.httpClient = http;
    console.log('BASE URL: ' + this.baseUrl);

    this._inventoriesServiceProxy.getCurrentTopup().subscribe(data => {
      this.currentTopup = data;
      console.log('=====================================');
      console.log(this.currentTopup);
      // console.log(this.currentTopup.isProcessing);
      this.isTopupProcessing = data.isProcessing;
    });

    http.get<any>(this.baseUrl + '/api/services/app/Products/GetAllItems').subscribe(data => {
      let plsSelect = new Product();
      plsSelect.id = '0';
      plsSelect.name = 'Select Product';
      plsSelect.price = 0;

      data.result.forEach((value) => {
        let item = new Product();
        item.id = value.product.id;
        item.name = value.product.name;
        item.price = value.product.price;
        this.products.push(item);
      });

      // this.products = data.result;
      this.products.unshift(plsSelect);
      console.log(this.products);
      // console.log(this.products);
    }, error => console.error(error));
  }

  insertInventory() {
    this.processing = true;
    this.tags.forEach(tag => {
      let data = new Inventory();
      data.tagId = tag.Id;
      data.productId = this.selectedProduct.id;
      data.price = this.selectedPrice;

      // Add to list
      this.inventories.push(data);
    });
    console.log(this.inventories);
    this.http.post<Inventory>(this.baseUrl + '/api/services/app/Inventories/Topup', this.inventories).subscribe(result => {
      this.totalTag = 0;
      //this.tags = new Array<TagId>();
      this.RefreshProduct();
      this.processing = false;
      this.tags = [];
      this.inventories = [];
      alert('Topup successful');

    }, error => {
      alert('Topup failed!');
      console.error(error);
      this.tags = [];
      this.inventories = [];
      this.processing = false;
    });
  }



  newTopup() {
    this._inventoriesServiceProxy.newTopup().subscribe(date => {
      console.log('New topup done');
      this.isTopupProcessing = true;
    });
  }

  endTopup() {
    this._inventoriesServiceProxy.endTopup(0).subscribe(data => {
      console.log('End topup done');
      this.isTopupProcessing = false;
    });
  }

  selected() {
    this.selectedPrice = this.selectedProduct.price;
    //console.log(this.selectedProduct);
  }

  ngOnInit() {
    this._inventoriesServiceProxy.getCurrentTopupInfo().subscribe(result => {
      this.currentTopupInfo = result;
      console.log('=============Current Top Info============');
      console.log(result);
    });

    this.topicSubscription = this.rxStompService.watch('/topic/tagid').subscribe((message: Message) => {
      if (!this.processing) {
        //this.receivedMessages.push(message.body);
        this.tags = JSON.parse(message.body);
        console.log(this.tags);
        this.totalTag = this.tags.length;
      }
    });
  }

  ngOnDestroy() {
    this.topicSubscription.unsubscribe();
  }

  OpenLock() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'FRID_OPEN' });
  }

  RefreshProduct() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'MACHINE_REFRESHPRD' });
  }

}

export class Inventory {
  id: string;
  tagId: string;
  trayLevel: string;
  price: number;
  product: Product;
  createdData: string;
  createdBy: string;
  updatedDate: string;
  updatedBy: string;
  isDeleted: boolean;
  productId: string;
}

export class TagId {
  TrayLevel: string;
  Id: string;
}

class Product {
  id: string;
  name: string;
  price: number;
}

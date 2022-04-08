import { Component, Inject, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { OnInit } from "@angular/core";
import { RxStompService } from '@stomp/ng2-stompjs';
import { Message } from '@stomp/stompjs';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-topup',
  templateUrl: './topup.component.html',
  styleUrls: ['./topup.component.css']
})


export class TopupComponent implements OnInit, OnDestroy {
  public products: Product[];
  public selectedProduct: Product;
  public totalTag: number;
  public receivedMessages: string[] = [];
  private topicSubscription: Subscription;
  private httpClient: HttpClient;
  private url: string;
  public selectedPrice : number;
  public tags: Array<TagId> = [];
  public inventories: Array<Inventory> = [];
  public processing: boolean;

  constructor(private rxStompService: RxStompService, private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.totalTag = 0;
    this.url = baseUrl;
    this.httpClient = http;
    http.get<Product[]>(baseUrl + 'api/Product/GetAll').subscribe(result => {
      var plsSelect = new Product();
      plsSelect.id = -1;
      plsSelect.productName = "Select Product";
      plsSelect.price = 0;
      this.products = result;
      this.products.unshift(plsSelect);

      console.log(this.products);
    }, error => console.error(error));
  }

  insertInventory() {
    this.processing = true;
    this.tags.forEach(tag => {
      var data = new Inventory();
      data.tagId = tag.Id;
      data.productId = this.selectedProduct.id;
      data.price = this.selectedPrice;

      // Add to list
      this.inventories.push(data)
    });
    console.log(this.inventories);
    this.http.post<Inventory>(this.url + 'api/Inventory/Insert', this.inventories).subscribe(result => {
      this.totalTag = 0;
      //this.tags = new Array<TagId>();
      this.RefreshProduct();
      this.processing = false;
      this.tags = [];
      this.inventories = [];
      alert("Topup successful");

    }, error => {
      alert("Topup failed!");
      console.error(error);
      this.tags = [];
      this.inventories = [];
      this.processing = false;
    });
  }

  selected() {
    this.selectedPrice = this.selectedProduct.price;
    //console.log(this.selectedProduct);
  }

  ngOnInit() {
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
    this.rxStompService.publish({ destination: '/topic/command', body: "FRID_OPEN" });
  }

  RefreshProduct() {
    this.rxStompService.publish({ destination: '/topic/command', body: "MACHINE_REFRESHPRD" });
  }

}

class Inventory {
  id: number;
  tagId: string;
  trayLevel: string;
  price: number;
  product: Product;
  createdData: string;
  createdBy: string;
  updatedDate: string;
  updatedBy: string;
  isDeleted: boolean;
  productId: number;
}

class TagId {
  TrayLevel: string;
  Id: string;
}

class Product {
  id: number;
  productName: string;
  price: number;
  createdData: string;
  createdBy: string;
  updatedDate: string;
  updatedBy: string;
  isDeleted: boolean;
}

class Test {
  Blah: string;
}

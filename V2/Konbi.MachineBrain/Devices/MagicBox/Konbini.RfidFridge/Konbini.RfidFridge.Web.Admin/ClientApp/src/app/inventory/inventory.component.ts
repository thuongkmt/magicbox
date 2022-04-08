import { Component, OnInit, Inject, AfterViewInit} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RxStompService } from '@stomp/ng2-stompjs';
import { Message } from '@stomp/stompjs';
import { Subscription } from 'rxjs';
import * as $ from 'jquery'

@Component({
  selector: 'app-inventory',
  templateUrl: './inventory.component.html',
  styleUrls: ['./inventory.component.css']
})
export class InventoryComponent implements OnInit, AfterViewInit {
  public inventories: Inventory[];
  public inven: any[];

  private topicSubscription: Subscription;


  constructor(private  httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string, private rxStompService: RxStompService) {
    this.reload();
  }

  reload() {
    this.httpClient.get<Inventory[]>(this.baseUrl + 'api/Inventory/GetAll').subscribe(result => {
      this.inventories = result;
      console.log(this.inventories)
    }, error => console.error(error));
  }

  delete(id) {
    var ok = confirm("Unload this product?");
    if (ok) {
      this.httpClient.post<Inventory>(this.baseUrl + 'api/Inventory/Delete', id).subscribe(result => {
        this.reload();
      }, error => {
        alert("Unload product failed!");
        console.error(error);
      });
    }
  }

  ngOnInit() {
    this.topicSubscription = this.rxStompService.watch('/topic/inventory').subscribe((message: Message) => {
      //console.log("=============================" + message.body)
      this.inven = JSON.parse(message.body);
      console.log(this.inven);
      for (let i = 0; i < this.inven.length; i++) {
        $('#' + this.inven[i].TagId).removeClass('table-danger');
      }
    });
  }
  ngAfterViewInit() {
  
  }
  

}

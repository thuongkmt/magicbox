import { Component, OnDestroy, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RxStompService } from '@stomp/ng2-stompjs';
import { Message } from '@stomp/stompjs';
import { Subscription } from 'rxjs';


@Component({
  selector: 'app-diagnostic',
  templateUrl: './diagnostic.component.html'
})
export class DiagnosticComponent implements OnInit, OnDestroy {
  public receivedMessages: string[] = [];
  private topicSubscription: Subscription;
  public inventories: Array<InventoryDto> = [];

  constructor(private rxStompService: RxStompService) { }

  ngOnInit() {
    this.topicSubscription = this.rxStompService.watch('/topic/inventory').subscribe((message: Message) => {
     //console.log("=============================" + message.body)
      this.receivedMessages.push(message.body);
      this.inventories = JSON.parse(message.body);
      console.log(this.inventories);
    });
  }

  ngOnDestroy() {
    this.topicSubscription.unsubscribe();
  }

  onSendMessage() {
    const message = `Message generated at ${new Date}`;
    this.rxStompService.publish({ destination: '/topic/command', body: message });
  }

  OpenLock() {
    //this.onSendMessage();
    this.rxStompService.publish({ destination: '/topic/command', body: "FRID_OPEN" });

  }

  EnableTerminal() {
    this.rxStompService.publish({ destination: '/topic/command', body: "TERMINAL_ENABLE" });

  }

  DisableTerminal() {
    this.rxStompService.publish({ destination: '/topic/command', body: "TERMINAL_DISABLE" });

  }

  RefreshProduct() {
    this.rxStompService.publish({ destination: '/topic/command', body: "MACHINE_REFRESHPRD" });
  }
  ClearInventory() {
    var ok = confirm("Are you sure want to clear inventory?");
    if (ok) {
      this.rxStompService.publish({ destination: '/topic/command', body: "MACHINE_CLRINVENT" });
    }
  }
  
}


class InventoryDto {
  TagId: string;
  TrayLevel: number;
  Id: number;
  Product: ProductDto;
}
class ProductDto {
  ProductName: string;
  SKU: string;
  Price: string;
}




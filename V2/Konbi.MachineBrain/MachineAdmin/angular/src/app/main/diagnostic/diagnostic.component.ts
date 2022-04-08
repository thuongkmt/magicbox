import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { Component, OnInit, Injector, OnDestroy } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { PaymentServiceProxy } from '@shared/service-proxies/payment-service-proxies';
import { CommonServiceProxy, GetComPortDtoList } from '@shared/service-proxies/common-service-proxies';
import { strict } from 'assert';
import { HttpClient } from '@angular/common/http';
import { RxStompService } from '@stomp/ng2-stompjs';
import { Message } from '@stomp/stompjs';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-diagnostic',
  templateUrl: './diagnostic.component.html',
  styleUrls: ['./diagnostic.component.css']
})
export class DiagnosticComponent extends AppComponentBase implements OnInit, OnDestroy {

  public receivedMessages: string[] = [];
  private invenTopicSubscription: Subscription;
  private tmpTopicSubscription: Subscription;
  public inventories: Array<InventoryDto> = [];
  public currentTmp: string;
  public tmpIndex: string;
  public tmpLastUpdate: string;
  public tmpData: any;
  constructor(
    private rxStompService: RxStompService,
    injector: Injector
  ) {
    super(injector);
  }


  ngOnInit() {
    this.currentTmp = '--';
    this.tmpIndex = '--';
    this.tmpLastUpdate = '--';

    this.invenTopicSubscription = this.rxStompService.watch('/topic/inventory').subscribe((message: Message) => {
      this.receivedMessages.push(message.body);
      this.inventories = JSON.parse(message.body);
    });

    this.tmpTopicSubscription = this.rxStompService.watch('/topic/temperatures').subscribe((message: Message) => {
      this.tmpData = JSON.parse(message.body);
      this.currentTmp = this.tmpData.Temp;
      this.tmpIndex = this.tmpData.Index;
      this.tmpLastUpdate = this.convertDateFromUnix(this.tmpData.LastUpdate);
     });
  }

  convertDateFromUnix(unix) {
    let d = new Date(unix * 1000).toLocaleDateString('en-US');
    let m  = new Date(unix * 1000).toLocaleTimeString('en-US');
    return d + ' ' + m;
  }

  ngOnDestroy() {
    this.invenTopicSubscription.unsubscribe();
    this.tmpTopicSubscription.unsubscribe();
  }

  onSendMessage() {
    const message = `Message generated at ${new Date}`;
    this.rxStompService.publish({ destination: '/topic/command', body: message });
  }

  OpenLock() {
    //this.onSendMessage();
    this.rxStompService.publish({ destination: '/topic/command', body: 'FRID_OPEN' });

  }

  EnableTerminal() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'TERMINAL_ENABLE' });
  }

  DisableTerminal() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'TERMINAL_DISABLE' });

  }

  OpenLatch() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'CARDHOLDER_OPENLATCH' });
  }

  RefreshCloudInventory() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'MACHINE_REFRESHCLOUDINVENT' });
  }


  RefreshProduct() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'MACHINE_REFRESHPRD' });
  }
  ClearInventory() {
    let ok = confirm('Are you sure want to clear inventory?');
    if (ok) {
      this.rxStompService.publish({ destination: '/topic/command', body: 'MACHINE_CLRINVENT' });
    }
  }
  StartTransaction() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'MACHINE_STARTTXN' });
  }
}



import { Component, OnInit, Injector, OnDestroy } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { RxStompService } from '@stomp/ng2-stompjs';
import { Message } from '@stomp/stompjs';
import { Subscription } from 'rxjs';
import { AlertSettingAppServicesServiceProxy, MachineStatus } from '@shared/service-proxies/alert-configuration-service-proxies';
import { finalize } from 'rxjs/operators';
import * as $ from 'jquery';

@Component({
  selector: 'app-unstable-tag-diagnostic',
  templateUrl: './unstable-tag-diagnostic.component.html',
  styleUrls: ['./unstable-tag-diagnostic.component.css']
})
export class UnstableTagDiagnosticComponent extends AppComponentBase implements OnInit, OnDestroy {
  private invenTopicSubscription: Subscription;
  private missedInvenTopicSubscription: Subscription;
  public unstableInventories: Array<UnstableInventoryDto> = [];
  public dumpMissedInventories: Array<InventoryDto> = [];
  public missedInventories: Array<InventoryDto> = [];
  public enRecord: boolean;
  public enClear: boolean;
  public enTrace: boolean;
  public enEnd: boolean;

  constructor(
    private rxStompService: RxStompService,
    private _alertSettingAppServicesServiceProxy: AlertSettingAppServicesServiceProxy,
    injector: Injector
  ) {
    super(injector);
  }

  ngOnInit() {
    this.enRecord = true;
    this.enClear = false;
    this.enTrace = false;
  }


  RecordUnstableTags() {
    this.enRecord = false;
    this.enClear = true;
    this.enTrace = true;
    this.changeMachineStatus(MachineStatus.UNSTABLE_TAGS_DIAGNOSTIC);

    this.UnSubInventoryTopic();
    this.SubUnstableInventoryTopic();
  }

  ClearRecord() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'CMD_CLEARUNSTABLERECORD' });
  }

  TracingTag() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'FRID_OPEN' });
    this.changeMachineStatus(MachineStatus.UNSTABLE_TAGS_DIAGNOSTIC_TRACING);
    this.UnSubInventoryTopic();
    this.SubMissedInventoryTopic();
    this.enClear = false;
    this.enTrace = false;
    this.unstableInventories = [];
  }

  OpenDoor() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'FRID_OPEN' });
  }

  End() {
    this.enRecord = true;
    this.enClear = false;
    this.enTrace = false;
    this.UnSubInventoryTopic();
    this.unstableInventories = [];
    this.missedInventories = [];
    this.changeMachineStatus(MachineStatus.IDLE);
  }


  ngOnDestroy(): void {
    this.UnSubInventoryTopic();
  }

  changeMachineStatus(status: MachineStatus) {
    this._alertSettingAppServicesServiceProxy.updateMachineStatus(status)
      .pipe(finalize(() => { }))
      .subscribe(() => {
        //this.notify.info(this.l('SavedSuccessfully'));
      });
  }

  SubUnstableInventoryTopic() {
    this.invenTopicSubscription = this.rxStompService.watch('/topic/unstable-inventory').subscribe((message: Message) => {
      console.log(message.body);
      this.unstableInventories = JSON.parse(message.body);
    });
  }

  SubMissedInventoryTopic() {
    this.invenTopicSubscription = this.rxStompService.watch('/topic/missed-inventory').subscribe((message: Message) => {
      this.missedInventories = JSON.parse(message.body);


      let tds = $("#unstable_table td.tagId");
      let tds1 = $("#takenout_table td.tagId");

      $.each(tds, function (i, val) {
        $(val).parent().removeClass("highlight");
      });
      $.each(tds1, function (i, val) {
        $(val).parent().removeClass("highlight");
      });

      this.unstableInventories.forEach(function (item) {
        let tagId = item.Inventory.TagId;
        $.each(tds1, function (i, val1) {
          var text = val1.innerText;
          if (text == tagId) {
            $(val1).parent().addClass("highlight");
          }
          else {
          }
        });
      });

      this.missedInventories.forEach(function (item) {
        let tagId = item.TagId;
        $.each(tds, function (i, val) {
          var text = val.innerText;
          if (text == tagId) {
            $(val).parent().addClass("highlight");
          }
        });
      });

    });
  }

  UnSubInventoryTopic() {
    if (this.invenTopicSubscription) {
      this.invenTopicSubscription.unsubscribe();
    }
    if (this.missedInvenTopicSubscription) {
      this.missedInvenTopicSubscription.unsubscribe();
    }
  }
}

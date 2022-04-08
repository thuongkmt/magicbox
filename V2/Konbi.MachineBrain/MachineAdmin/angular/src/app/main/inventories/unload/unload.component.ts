import { Component, OnInit, Injector, OnDestroy } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { RxStompService } from '@stomp/ng2-stompjs';
import { Message } from '@stomp/stompjs';
import { Subscription } from 'rxjs';
import { finalize } from 'rxjs/operators';
import * as $ from 'jquery';
import { ProductsServiceProxy, ProductDto, GetProductForViewDto } from '@shared/service-proxies/service-proxies';
import { MachineStatus, AlertSettingAppServicesServiceProxy } from '@shared/service-proxies/alert-configuration-service-proxies';

@Component({
  selector: 'app-unload',
  templateUrl: './unload.component.html',
  styleUrls: ['./unload.component.css']
})
export class UnloadComponent extends AppComponentBase implements OnInit, OnDestroy {
  public products: GetProductForViewDto[] = [];
  public selectedProduct: GetProductForViewDto;
  public missedInventories: Array<InventoryDto> = [];
  public enStart: boolean;
  public enEnd: boolean;
  public enUnload: boolean;

  private missedInvenTopicSubscription: Subscription;

  constructor(
    private rxStompService: RxStompService,
    private _productsServiceProxy: ProductsServiceProxy,
    private _alertSettingAppServicesServiceProxy: AlertSettingAppServicesServiceProxy,
    injector: Injector
  ) {
    super(injector);
  }
  ngOnInit() {
    this.onLoad();
  }
  onLoad() {
    this._productsServiceProxy.getAllItems().subscribe(data => {
      this.products = data;

      let plsSelect = new GetProductForViewDto();
      plsSelect.product = new ProductDto();
      plsSelect.product.id = '0';
      plsSelect.product.name = '==SELECT PRODUCT==';

      this.products.unshift(plsSelect);
    });
    this.enStart = true;
  }

  onProductChange() {
    let isEnable = (this.selectedProduct.product.id != '0')
    this.enUnload = isEnable;
  }

  startUnloadItem() {
    this.openDoor();
    this.changeMachineStatus(MachineStatus.UNLOADING_PRODUCT);
    this.subMissedInventory();

    this.enStart = false;
    this.enEnd = true;
  }

  endUnloadItem() {
    this.unSubMissedInventory();
    this.changeMachineStatus(MachineStatus.IDLE);

    this.enStart = true;
    this.enEnd = false;
  }

  openDoor() {
    this.rxStompService.publish({ destination: '/topic/command', body: 'FRID_OPEN' });
  }

  unloadProduct() {

  }
  unloadAll() {

  }
  subMissedInventory() {
    this.missedInvenTopicSubscription = this.rxStompService.watch('/topic/missed-inventory').subscribe((message: Message) => {
      this.missedInventories = JSON.parse(message.body);
    });
  }

  unSubMissedInventory() {
    if (this.missedInvenTopicSubscription) {
      this.missedInvenTopicSubscription.unsubscribe();
    }
  }


  changeMachineStatus(status: MachineStatus) {
    this._alertSettingAppServicesServiceProxy.updateMachineStatus(status)
      .pipe(finalize(() => { }))
      .subscribe(() => {
        //this.notify.info(this.l('SavedSuccessfully'));
      });
  }


  ngOnDestroy(): void {
    this.unSubMissedInventory();
  }

}

import { Component, OnInit, Injector, Optional, Inject, ViewChild} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppComponentBase } from '@shared/app-component-base';
import { API_BASE_URL, EntityDto } from '@shared/service-proxies/service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { MachineLoadoutServiceProxy, LoadoutDto, LoadoutItem, RestockSession} from '@shared/service-proxies/machineLoadout-service-proxies';
import { ChangeProductModal } from './change-product-modal.component';
import { EditLoadoutModal } from './edit-loadout-modal.component';
import { EditLoadoutItemModal } from './edit-loadoutitem-modal.component';
import * as lodash from 'lodash';

@Component({
  selector: 'app-machine-loadout',
  templateUrl: './machine-loadout.component.html',
  styleUrls: ['./machine-loadout.component.css'],
  animations: [appModuleAnimation()]
})

export class MachineLoadoutComponent extends AppComponentBase implements OnInit {
    @ViewChild('changeProductModal') changeProductModal: ChangeProductModal;
    @ViewChild('editLoadoutModal') editLoadoutModal: EditLoadoutModal;
    @ViewChild('editLoadoutItemModal') editLoadoutItemModal: EditLoadoutItemModal;

    constructor(
        injector: Injector,
        private route: ActivatedRoute,       
        private machineLoadoutService: MachineLoadoutServiceProxy,
        @Optional() @Inject(API_BASE_URL) baseRemoteUrl?: string,
    ) {
        super(injector);
        this.baseRemoteUrl = baseRemoteUrl ? baseRemoteUrl : "";
    }

    loadout: LoadoutDto = new LoadoutDto();
    currentMachineID: any;
    currentProductId: any
    saving: boolean = false;
    baseRemoteUrl: string;
    currencySymbol: string = "";
    isRestocking = false;

    ngOnInit() {
        this.currentMachineID = this.route.snapshot.paramMap.get('id');
        this.currencySymbol = abp.setting.get("CurrencySymbol");
        this.getMachineLoadout();
        this.getResctokSession();
    }

    getProductNameDisplay(productName: string) {
        if (productName === undefined || productName === null)
            return productName;
        return productName.length < 15 ? productName : productName.substring(0, 15) + '...';
    }
    getMachineLoadout(): void {
        abp.ui.setBusy();
        this.machineLoadoutService.getMachineLoadout(this.currentMachineID)
        .finally(() => {
            abp.ui.clearBusy();
        })
        .subscribe((result: LoadoutDto) => {
            this.loadout = result;
            if(result.loadoutList.length == 0)
            {
                this.loadout.isOnline = false;
            }
            abp.ui.clearBusy();
        });
    }

    getResctokSession(): void {
        abp.ui.setBusy();
        this.machineLoadoutService.getRestockSession(this.currentMachineID)
        .finally(() => {
            abp.ui.clearBusy();
        })
        .subscribe((rs: RestockSession) => {
            if(rs.isInprogress)
            {
                this.isRestocking = true;
            }
            abp.ui.clearBusy();
        });
    }

    delete(item: LoadoutItem): void {
        abp.message.confirm( "Are you sure want to delete?", "Delete",
            (resultConfirm: boolean) => {
                if (resultConfirm) {
                    this.saving = true;
                    abp.ui.setBusy();
                    var deleteItem = lodash.cloneDeep(item);
                    deleteItem.productId = "";
                    deleteItem.price = 0;
                    deleteItem.quantity = 0;
                    this.machineLoadoutService.updateLoadoutItem(deleteItem)
                    .finally(() => 
                    {
                         this.saving = false;
                         abp.ui.clearBusy();
                     })
                    .subscribe((result: boolean) => {
                        if (result) {
                            this.getMachineLoadout();
                            this.notify.info(this.l('Deleted Successfully'));
                        }
                    });
                }
            }
        );
    }

    hasProduct(item: LoadoutItem)
    {
        return item.productId != undefined && item.productId != null;
    }

    onAssignProducts(item: LoadoutItem)
    {
        this.changeProductModal.show(this.loadout, this.baseRemoteUrl);
    }

    onEditLoadout()
    {
        this.editLoadoutModal.show(this.loadout, this.baseRemoteUrl);
    }

    onEditLoadoutItem(item: LoadoutItem)
    {
        this.editLoadoutItemModal.show(item,
                                    this.baseRemoteUrl,
                                    this.currentMachineID);
    }

    startRestock()
    {
        abp.ui.setBusy();
        var input = new EntityDto();
        input.id = this.currentMachineID;
        this.machineLoadoutService.startRestock(input)
        .finally(() => 
        {
            abp.ui.clearBusy();
        })
        .subscribe((result: boolean) => {
            if (result) {
                this.isRestocking = true;
                abp.ui.clearBusy();
            }
        });
    }

    endRestock()
    {
        abp.ui.setBusy();
        var input = new EntityDto();
        input.id = this.currentMachineID;
        this.machineLoadoutService.endRestock(input)
        .finally(() => 
        {
            abp.ui.clearBusy();
        })
        .subscribe((result: boolean) => {
            if (result) {
                abp.ui.clearBusy();
                this.isRestocking = false;
            }
        });
    }
}

import { Component, ViewChild, Injector, Output, EventEmitter} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { AppComponentBase } from '@shared/common/app-component-base';
import { MachineLoadoutServiceProxy, LoadoutDto, LoadoutList, UpdateLoadoutDto } from '@shared/service-proxies/machineLoadout-service-proxies';
import * as lodash from 'lodash';

@Component({
    selector: 'editLoadoutModalSelector',
    templateUrl: './edit-loadout-modal.component.html'
})
export class EditLoadoutModal extends AppComponentBase {

    @ViewChild('editLoadoutModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    currentMachineID: any;
    currencySymbol: string = "";
    loadout: LoadoutDto = new LoadoutDto();
    baseRemoteUrl: string = "";

    editPrice: boolean = false;
    editQuantity: boolean = false;
    editCapacity: boolean = false;

    price:number = 0;
    quantity:number = 0;
    capacity:number = 0;

    constructor(
        injector: Injector,
        private loadoutService: MachineLoadoutServiceProxy
    ) {
        super(injector);
    }

    show(loadout, baseRemoteUrl): void {
        this.currencySymbol = abp.setting.get("CurrencySymbol");
        this.loadout = lodash.cloneDeep(loadout)
        this.baseRemoteUrl = baseRemoteUrl;

        this.editPrice = false;
        this.editQuantity = false;
        this.editCapacity = false;
        this.active = true;
        this.modal.show();
    }

    save(): void {
        if(!this.editQuantity || !this.editCapacity || !this.editPrice)
        {
            return;
        }
        if(this.editQuantity && this.editCapacity && this.quantity > this.capacity){
            this.message.error("Quantity cannot greater than capacity");
            return;
        }
        abp.ui.setBusy();
        var updateDto = new UpdateLoadoutDto();
        updateDto.machineId = this.loadout.machineId;
        updateDto.loadouts = [];
        this.loadout.loadoutList.forEach(item =>
        {
            item.loadouts.forEach(loadout => 
            {
                if(loadout.isChecked)
                {
                    var hasChange = false;
                    if(this.editPrice)
                    {
                        if(loadout.price != this.price)
                        {
                            loadout.price = this.price;
                            hasChange = true;
                        }
                    }
                    if(this.editQuantity)
                    {
                        if(loadout.quantity != this.quantity)
                        {
                            loadout.quantity = this.quantity;
                            hasChange = true;
                        }
                    }
                    if(this.editCapacity)
                    {
                        if(loadout.capacity != this.capacity)
                        {
                            loadout.capacity = this.capacity;
                            hasChange = true;
                        }
                    }
                    if(hasChange)
                    {
                        updateDto.loadouts.push(loadout);
                    }
                }
            });
        });
        if(updateDto.loadouts.length > 0)
        {
            this.saving = true;
            this.loadoutService.updateLoadout(updateDto)
            .finally(() => {
                this.saving = false;
                abp.ui.clearBusy();
            })
            .subscribe(() => {
                abp.ui.clearBusy();
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
            });
            this.editPrice = false;
            this.editQuantity = false;
            this.editCapacity = false;

            this.price = 0;
            this.quantity = 0;
            this.capacity = 0;
        }
        else
        {
            abp.ui.clearBusy();
        }
    }

    close(): void
    {
        this.active = false;
        this.modal.hide();
    }

    checkAllChange(item: LoadoutList)
    {
        item.loadouts.forEach(lo => 
        {
            lo.isChecked = item.checkAll;
        });
    }
}

import { Component, Injector, NgZone, OnInit, OnDestroy } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import 'rxjs/add/operator/takeWhile';
import { MachineServiceProxy, PagedResultDtoOfMachineStatusDto, MachineStatusDto } from "shared/service-proxies/machine-service-proxies";
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { SignalRHelper } from 'shared/helpers/SignalRHelper';
import { MagicBoxSignalrService } from '../../../shared/common/signalr/magicbox-signalr.service';

@Component({
    selector: 'app-machine-statuses',
    templateUrl: './machine-statuses.component.html',
    styleUrls: ['./machine-statuses.component.css'],
    animations: [appModuleAnimation()]
})
export class MachineStatusesComponent extends AppComponentBase implements OnInit, OnDestroy {
    private alive: boolean;
    constructor(
        private injector: Injector,
        public machinesService: MachineServiceProxy,
        private _magicBoxSignalrService: MagicBoxSignalrService,
        public _zone: NgZone
    ) {
        super(injector);
        this.alive = true;
    }

    machineStatus: MachineStatusDto[] = [];

    ngOnInit() {
        if (this.appSession.application) {
            SignalRHelper.initSignalR(() => {
                this._magicBoxSignalrService.init();
            });
        }
        this.registerEvents();
        abp.ui.setBusy('#machineStatus');
        this.machinesService.getAllMachineStatus(new MachineStatusDto())
        .finally(() => {
            abp.ui.clearBusy('#machineStatus');
        })
        .subscribe((result: PagedResultDtoOfMachineStatusDto) => {
            this.machineStatus = result.items;
            abp.ui.clearBusy('#machineStatus');
        })
    }

    ngOnDestroy() {
        this.alive = false;
    }

    registerEvents(): void {
        const self = this;
        function onMessageReceived(message:any) {
            var input = new MachineStatusDto();
            input.MachineId = message;
            self.machinesService.getAllMachineStatus(input)
            .finally(() => {
            })
            .subscribe((result: PagedResultDtoOfMachineStatusDto) => {
                var newStatus = result.items[0];
                var oldStatus = self.machineStatus.find(x => x.MachineId == message);
                var index = self.machineStatus.indexOf(oldStatus);
                if(index > -1)
                {
                    self.machineStatus[index] = newStatus;
                }
            })
        };

        abp.event.on('MagicBoxMachineStatusMessage', message => {
            self._zone.run(() => {
                onMessageReceived(message);
            });
        });
    }
}

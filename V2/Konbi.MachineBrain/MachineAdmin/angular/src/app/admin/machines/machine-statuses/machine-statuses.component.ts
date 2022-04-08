import { Component, Injector, ViewChild, OnInit, OnDestroy } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { IntervalObservable } from "rxjs/observable/IntervalObservable";
import 'rxjs/add/operator/takeWhile';
import { MachineServiceProxy, PagedResultDtoOfMachineStatusDto, MachineStatusDto } from "shared/service-proxies/machine-service-proxies";
import { appModuleAnimation } from '@shared/animations/routerTransition';


@Component({
    selector: 'app-machine-statuses',
    templateUrl: './machine-statuses.component.html',
    styleUrls: ['./machine-statuses.component.css'],
    animations: [appModuleAnimation()]
})
export class MachineStatusesComponent extends AppComponentBase implements OnInit, OnDestroy {
    private alive: boolean;
    private interval: number;
    constructor(
        private injector: Injector,
        private machinesService: MachineServiceProxy
    ) {
        super(injector);
        this.alive = true;
        this.interval = 10000000;
    }

    machineStatus: MachineStatusDto[] = [];
    ngOnInit() {
        this.machinesService.getAllMachineStatus()
            .finally(() => {
            })
            .subscribe((result: PagedResultDtoOfMachineStatusDto) => {
                this.machineStatus = result.items;
            })
        IntervalObservable.create(this.interval)
            .takeWhile(() => this.alive)
            .subscribe(() => {
                this.autoRefresh();
            });
    }
    autoRefresh() {
        this.machinesService.getAllMachineStatus()
            .finally(() => {
            })
            .subscribe((result: PagedResultDtoOfMachineStatusDto) => {

                this.machineStatus = result.items;
            })
    }
    ngOnDestroy() {
        this.alive = false;
    }
}

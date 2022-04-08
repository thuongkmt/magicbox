import { Component, Injector, ViewChild } from '@angular/core';
import { Router } from "@angular/router";
import { PagedListingComponentBase, PagedRequestDto } from "shared/paged-listing-component-base";

import { MachineServiceProxy, MachineDto, PagedResultDtoOfMachineDto } from "shared/service-proxies/machine-service-proxies";

import { appModuleAnimation } from '@shared/animations/routerTransition';

import { CreateMachineComponent } from "./create-machine/create-machine.component";
import { EditMachineComponent } from "./edit-machine/edit-machine.component";
import { RemoteSettingsComponent } from "./remote-settings/remote-settings.component";


import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { Paginator } from 'primeng/components/paginator/paginator';
import { Table } from 'primeng/components/table/table';

@Component({
    selector: 'app-machines',
    templateUrl: './machines.component.html',
    styleUrls: ['./machines.component.css'],
    animations: [appModuleAnimation()]
})
export class MachinesComponent extends PagedListingComponentBase<MachineDto> {
    @ViewChild('createMachineModal') createMachineModal: CreateMachineComponent;
    @ViewChild('editMachineModal') editMachineModal: EditMachineComponent;
    //  @ViewChild('remoteSettingsModel') remoteSettingsModel: RemoteSettingsComponent;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;

    machines: MachineDto[] = [];

    constructor(
        private injector: Injector,
        private router: Router,
        private machinesService: MachineServiceProxy
    ) {
        super(injector);
    }

    getMachines(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);

            return;
        }
        this.primengTableHelper.showLoadingIndicator();

        this.machinesService.getAll(
            this.primengTableHelper.getSkipCount(this.paginator, event),
            this.primengTableHelper.getMaxResultCount(this.paginator, event),
            this.primengTableHelper.getSorting(this.dataTable)
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
        
        // this.machinesService.getAll(request.skipCount, request.maxResultCount)
        //     .finally(() => {
        //         finishedCallback();
        //     })
        //     .subscribe((result: PagedResultDtoOfMachineDto) => {
        //         this.machines = result.items;
        //         this.showPaging(result, pageNumber);
        //     });
    }

    delete(Machine: MachineDto): void {
        abp.message.confirm(
            "Remove machines from Machine and delete Machine '" + Machine.name + "'?",
            "Permanently delete this Machine",
            (result: boolean) => {
                if (result) {
                    this.machinesService.delete(Machine.id)
                        .finally(() => {
                            abp.notify.info("Deleted Machine: " + Machine.name);
                            this.getMachines();
                        })
                        .subscribe(() => { });
                }
            }
        );
    }

    sendCommand(Machine: MachineDto): void {
        console.log('send command ');
        //this.remoteSettingsModel.show(Machine.id);
        this.router.navigate(['/app/machines/RemoteSettings/' + Machine.id]);
        //abp.message.confirm(
        //    "Remove machines from Machine and delete Machine '" + Machine.name + "'?",
        //    "Permanently delete this Machine",
        //    (result: boolean) => {
        //        if (result) {
        //            this.machinesService.delete(Machine.id)
        //                .finally(() => {
        //                    abp.notify.info("Deleted Machine: " + Machine.name);
        //                    this.refresh();
        //                })
        //                .subscribe(() => { });
        //        }
        //    }
        //);
    }

    createMachine(): void {
        this.createMachineModal.show();
    }
    editMachine(Machine: MachineDto): void {
        this.editMachineModal.show(Machine.id);
    }

}

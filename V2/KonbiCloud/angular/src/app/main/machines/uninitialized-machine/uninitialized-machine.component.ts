import { Component, OnInit, ViewChild, Injector, NgZone } from '@angular/core';
import { Table } from 'primeng/table';
import { Paginator, LazyLoadEvent } from 'primeng/primeng';
import { MachineDto, MachineServiceProxy } from '@shared/service-proxies/machine-service-proxies';
import { Router, ActivatedRoute } from '@angular/router';
import { AppComponentBase } from '@shared/common/app-component-base';
import { SignalRHelper } from '@shared/helpers/SignalRHelper';
import { MagicBoxSignalrService } from '@app/shared/common/signalr/magicbox-signalr.service';

@Component({
  selector: 'app-uninitialized-machine',
  templateUrl: './uninitialized-machine.component.html',
  styleUrls: ['./uninitialized-machine.component.css']
})
export class UninitializedMachineComponent extends AppComponentBase {

  @ViewChild('dataTable') dataTable: Table;
  @ViewChild('paginator') paginator: Paginator;

  machines: MachineDto[] = [];

  constructor(
    private injector: Injector,
    private router: Router,
    private machinesService: MachineServiceProxy,
    private _magicBoxSignalrService: MagicBoxSignalrService,
    private _route: ActivatedRoute,
    public _zone: NgZone,
  ) {
    super(injector);
  }

  ngOnInit(): void {

    if (this.appSession.application) {
      SignalRHelper.initSignalR(() => {
        this._magicBoxSignalrService.init();
      });
    }
    this.registerEvents();
  }

  registerEvents(): void {
    const self = this;
    function onMessageReceived(message: any) {
        console.log(message);
    };

    abp.event.on('Topup', message => {
      self._zone.run(() => {
        onMessageReceived(message);
      });
    });

    abp.event.on('Transaction', message => {
      self._zone.run(() => {
        onMessageReceived(message);
      });
    });

    abp.event.on('CurrentInventory', message => {
      self._zone.run(() => {
        onMessageReceived(message);
      });
    });
  }

  getMachines(event?: LazyLoadEvent) {
    if (this.primengTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);

      return;
    }
    this.primengTableHelper.showLoadingIndicator();

    this.machinesService.GetAllUnintialized(
      this.primengTableHelper.getSkipCount(this.paginator, event),
      this.primengTableHelper.getMaxResultCount(this.paginator, event),
      this.primengTableHelper.getSorting(this.dataTable)
    ).subscribe(result => {
      this.primengTableHelper.totalRecordsCount = result.totalCount;
      this.primengTableHelper.records = result.items;
      this.primengTableHelper.hideLoadingIndicator();
    });
  }



  showDevices(machine: MachineDto): void {
    this.router.navigate(['/app/main/ShowDevices/' + machine.id + "/" + machine.name]);
  }

  initializeMachine(Machine: MachineDto): void {
    abp.message.confirm(
      "Would you like to initialize the machine: '" + Machine.name + "'?",
      "Initialize Machine",
      (result: boolean) => {
        if (result) {
          this.machinesService.initializeMachine(Machine.id)
            .finally(() => {
              abp.notify.info("Initialized Machine: " + Machine.name);
              this.getMachines();
            })
            .subscribe(() => { });
        }
      }
    );
  }
}

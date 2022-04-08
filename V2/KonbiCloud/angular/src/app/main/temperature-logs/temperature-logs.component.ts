import { Component, Injector, ViewEncapsulation, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TemperatureLogsServiceProxy, TemperatureListDto, ListResultDtoOfTemperatureListDto } from '@shared/service-proxies/temperature-logs-service-proxies';
import { MachineServiceProxy, MachineDto, PagedResultDtoOfMachineDto } from "shared/service-proxies/machine-service-proxies";
import { ActivatedRoute } from '@angular/router';
import * as moment from 'moment';
import * as _ from 'lodash';
import { element } from '@angular/core/src/render3';

@Component({
  selector: 'app-temperature-logs',
  templateUrl: './temperature-logs.component.html',
  styleUrls: ['./temperature-logs.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})
export class TemperatureLogsComponent extends AppComponentBase implements OnInit {

  filter = '00000000-0000-0000-0000-000000000000';
  dateFilter = '';
  legendTitle = 'Machine Name';
  xAxisLabel = 'Times';
  yAxisLabel = 'Temperature °C';
  temperatureLogs = [];

  dropdownList = [];
  selectedItems = [];
  dropdownSettings = {};

  constructor(
    injector: Injector,
    private route: ActivatedRoute,
    private _temperatureLogsService: TemperatureLogsServiceProxy,
    private _machinesService: MachineServiceProxy
  ) {
      super(injector);
  }

  // Init.
  ngOnInit() {
    this.getMachines();
    this.getTemperatureLogs();

    this.dropdownSettings = {
      singleSelection: false,
      idField: 'item_id',
      textField: 'item_text',
      selectAllText: 'Select All',
      unSelectAllText: 'UnSelect All',
      itemsShowLimit: 3,
      allowSearchFilter: true
    };
  }

  LoadChart() {
    this.filter = '';
    this.filter = _.map(this.selectedItems, 'item_id').join(',');
    this.getTemperatureLogs();
  }

  // Event select single machine.
  onItemSelect(item: any) {
    if (this.selectedItems.length > 0) {
      this.LoadChart();
    }
  }

  onDeSelect(item: any) {
    if (this.selectedItems.length > 0) {
      _.reject(this.selectedItems, item);
      this.LoadChart();
    } else {
      this.filter = '00000000-0000-0000-0000-000000000000';
      this.getTemperatureLogs();
    }
  }

  // Event select all machine.
  onSelectAll(items: any) {
    this.filter = '';
    this.filter = _.map(items, 'item_id').join(',');
    this.getTemperatureLogs();
  }

  // Event unselect all machine.
  onDeSelectAll(items: any) {
    this.filter = '00000000-0000-0000-0000-000000000000';
    this.getTemperatureLogs();
  }

  // Get all machines.
  getMachines(): void {
    this._machinesService.getAll(0, 999, 'name asc').subscribe((result) => {
      let machines = [];
      for (let entry of result.items) {
        let ChartLineItem = {
          'item_id': entry.id,
          'item_text': entry.name
        };
        machines.push(ChartLineItem);
      }
      this.dropdownList = machines;
    });
  }

  // Get TemperatureLogs by machine id.
  getTemperatureLogs(): void {
    this.primengTableHelper.showLoadingIndicator();
    this._temperatureLogsService.getTemperatureLogs(
      this.filter,
      this.dateFilter
    ).subscribe((result) => {
        this.formatDataForChartLine(result.items);
        this.primengTableHelper.hideLoadingIndicator();
    });
  }

  // Distinct.
  distinctTemp = (value, index, self) => {
    return self.indexOf(value) === index;
  }

  formatDate(val) {
      return val.format('LT');
  }

  // Format data return API for chart line.
  formatDataForChartLine(dataTemp: TemperatureListDto[]): void {
    // Reset array machine.
    let machineTemp = [];
    let dataChart = [];

    // Get all machine name.
    for (let entry of dataTemp) {
      machineTemp.push(entry.machineName);
    }
    machineTemp = machineTemp.filter(this.distinctTemp);

    // Add data for chart line.
    for (let entry of machineTemp) {
      let ChartLineItem = {
        'name': entry,
        'series': []
      };
      for (let item of dataTemp) {
        if (item.machineName === entry) {
          let ChartLineDataItem = {
            'value': item.temperature,
            'name': item.creationTime
          };
          ChartLineItem.series.push(ChartLineDataItem);
        }
      }
      dataChart.push(ChartLineItem);
    }
    this.temperatureLogs = dataChart;
  }
}

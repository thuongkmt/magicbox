import { Component, Injector, ViewEncapsulation, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TemperatureLogsServiceProxy, TemperatureListDto } from '@shared/service-proxies/temperature-logs-service-proxies';
import { ActivatedRoute, Router } from '@angular/router';
import * as moment from 'moment';
import * as _ from 'lodash';

@Component({
  selector: 'app-temperature-logs',
  templateUrl: './temperature-logs.component.html',
  styleUrls: ['./temperature-logs.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})
export class TemperatureLogsComponent extends AppComponentBase implements OnInit {

  interval: any;
  legendTitle = 'Machine Name';
  xAxisLabel = 'Times';
  yAxisLabel = 'Temperature °C';
  temperatureLogs = [];
  haveData = true;
  dateFilter = '';

  constructor(
    injector: Injector,
    private route: ActivatedRoute,
    private router: Router,
    private _temperatureLogsService: TemperatureLogsServiceProxy,
  ) {
    super(injector);
  }

  // Init form.
  ngOnInit() {
    // Get TemperatureLogs.
    this.getTemperatureLogs();
  }

  // Get TemperatureLogs.
  getTemperatureLogs(): void {
    this.primengTableHelper.showLoadingIndicator();
    this._temperatureLogsService.getTemperatureLogs(
      this.dateFilter
    ).subscribe((result) => {
      this.formatDataForChartLine(result.items);
      this.primengTableHelper.hideLoadingIndicator();
    });
  }

  formatDate(val) {
    return val.format('LT');
  }

  // Format data return API for chart line.
  formatDataForChartLine(dataTemp: TemperatureListDto[]): void {
    // Reset array machine.
    let dataChart = [];

    // Add data for chart line.
    let ChartLineItem = {
      'name': 'Temperature',
      'series': []
    };
    for (let item of dataTemp) {
      let ChartLineDataItem = {
        'value': item.temperature,
        'name': item.creationTime
      };
      ChartLineItem.series.push(ChartLineDataItem);
    }
    dataChart.push(ChartLineItem);

    this.haveData = dataChart[0].series.length > 0 ? true : false;

    this.temperatureLogs = dataChart;
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.interval = setInterval(() => {
        if (this.router.url === '/app/main/temperature-logs') {
          // Get TemperatureLogs.
          this.getTemperatureLogs();
        }
      }, 1000 * 30);
    });
  }
}

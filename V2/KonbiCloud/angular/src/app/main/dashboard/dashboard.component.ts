import { AfterViewInit, Component, Injector, ViewEncapsulation } from '@angular/core';
import { AppSalesSummaryDatePeriod } from '@shared/AppEnums';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { DashboardServiceProxy, TransactionForToday, MachineStatData } from '@shared/service-proxies/dashboard-service-proxies';
import { curveBasis } from 'd3-shape';
import { Router } from "@angular/router";
import { environment } from 'environments/environment';

import * as _ from 'lodash';
declare let d3, Datamap: any;



abstract class DashboardChartBase {
    loading = false;

    showLoading() {
        //setTimeout(() => { this.loading = true; });
    }

    hideLoading() {
        //setTimeout(() => { this.loading = false; });
    }
}

class DashboardHeaderStats extends DashboardChartBase {

    totalTransSale = 0;
    totalTransToday = 0;
    totalTransCurrentSession = 0;
    totalTransCurrentSessionSale = 0;
    transactionForToday = [];
  
    init(totalTransSale, totalTransToday, totalTransCurrentSession, totalTransCurrentSessionSale, transactionForToday) {
        this.totalTransSale = totalTransSale;
        this.totalTransToday = totalTransToday;
        this.totalTransCurrentSession = totalTransCurrentSession;
        this.totalTransCurrentSessionSale = totalTransCurrentSessionSale;
        this.transactionForToday = transactionForToday;

   
        this.hideLoading();
    }
}

class SalesSummaryChart extends DashboardChartBase {
    totalSales = 0;
    totalTrans = 0;

    selectedDatePeriod: AppSalesSummaryDatePeriod = AppSalesSummaryDatePeriod.Daily;

    data = [];

    constructor(
        private _dashboardService: DashboardServiceProxy,
        private _containerElement: any) {
        super();
    }

    init(salesSummaryData) {
        this.totalSales = 0;
        this.totalTrans = 0;
        salesSummaryData.forEach(element => {
            this.totalSales += element.sales;
            this.totalTrans += element.trans;
        });
        this.setChartData(salesSummaryData);
        this.hideLoading();
    }

    setChartData(items): void {
        let sales = [];
        let trans = [];

        _.forEach(items, (item) => {

            sales.push({
                'name': item['period'],
                'value': item['sales'].toFixed(2)
            });

            trans.push({
                'name': item['period'],
                'value': item['trans']
            });
        });

        this.data = [
            {
                'name': 'Sales',
                'series': sales
            }, {
                'name': 'Transactions',
                'series': trans
            }
        ];
    }

    reload(datePeriod) {
        this.selectedDatePeriod = datePeriod;
        this.showLoading();
        this._dashboardService
            .getSalesSummary(datePeriod)
            .subscribe(result => {
                this.init(result.salesSummary);
                this.hideLoading();
            });
    }
}

class MachineStatsTable extends DashboardChartBase {
    public view: any[] = [, 260];
    public single = [];

    constructor(private _dashboardService: DashboardServiceProxy) {
        super();
    }

    init() {
        this.reload();
    }

    formatData(stats: MachineStatData[]): any {
        this.single = [];
        for (let i = 0; i < stats.length; i++) {
            this.single.push({ 'name': stats[i].machineName, 'value': stats[i].totalSale.toFixed(2) });
        }
    }

    reload() {
        this._dashboardService
            .getMachineStats()
            .subscribe(result => {
                this.formatData(result.machineStat);
            });
    }
}

@Component({
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.less'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class DashboardComponent extends AppComponentBase implements AfterViewInit {

    interval: any;
    env = environment;
    legendTitle = 'Machines Name';

    appSalesSummaryDateInterval = AppSalesSummaryDatePeriod;
    selectedSalesSummaryDatePeriod: AppSalesSummaryDatePeriod = AppSalesSummaryDatePeriod.Daily;

    dashboardHeaderStats: DashboardHeaderStats;
    salesSummaryChart: SalesSummaryChart;
    machineStatsTable: MachineStatsTable;
    transactionForToday: TransactionForToday[] = [];
    currencySymbol = '$';

    constructor(
        injector: Injector,
        private router: Router,
        private _dashboardService: DashboardServiceProxy
    ) {
        super(injector);
        this.dashboardHeaderStats = new DashboardHeaderStats();
        this.salesSummaryChart = new SalesSummaryChart(this._dashboardService, 'salesStatistics');
        this.machineStatsTable = new MachineStatsTable(this._dashboardService);

        this.currencySymbol = abp.setting.get("CurrencySymbol");
        console.log("currencySymbol: " + this.currencySymbol);
    }

    getDashboardStatisticsData(datePeriod): void {
        this.primengTableHelper.showLoadingIndicator();
        this.salesSummaryChart.showLoading();
        this._dashboardService
            .getDashboardData(datePeriod)
            .subscribe(result => {
                this.dashboardHeaderStats.init(result.totalTransSale, result.totalTransToday, result.totalTransCurrentSession, result.totalTransCurrentSessionSale, result.transactionForToday);
                this.salesSummaryChart.init(result.salesSummary);
                this.primengTableHelper.hideLoadingIndicator();
            });
        this.machineStatsTable.init();
    }

    ngAfterViewInit(): void {
        setTimeout(() => {
            this.getDashboardStatisticsData(this.salesSummaryChart .selectedDatePeriod);
            this.interval = setInterval(() => {
                if (this.router.url === '/app/main/dashboard') {
                    this.getDashboardStatisticsData(this.salesSummaryChart.selectedDatePeriod);
                }
            }, 1000 * 30);
        });
    }
}

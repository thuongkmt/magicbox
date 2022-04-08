import { AfterViewInit, Component, Injector, ViewEncapsulation } from '@angular/core';
import { AppSalesSummaryDatePeriod } from '@shared/AppEnums';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { DashboardServiceProxy } from '@shared/service-proxies/dashboard-service-proxies';
import { curveBasis } from 'd3-shape';

import * as _ from 'lodash';
declare let d3, Datamap: any;


abstract class DashboardChartBase {
    loading = true;

    showLoading() {
        setTimeout(() => { this.loading = true; });
    }

    hideLoading() {
        setTimeout(() => { this.loading = false; });
    }
}

class DashboardHeaderStats extends DashboardChartBase {

    totalTransSale = 0;
    totalTransToday = 0;
    totalTransCurrentSession = 0;
    totalTransCurrentSessionSale = 0;

    init(totalTransSale, totalTransToday, totalTransCurrentSession, totalTransCurrentSessionSale) {
        this.totalTransSale = totalTransSale;
        this.totalTransToday = totalTransToday;
        this.totalTransCurrentSession = totalTransCurrentSession;
        this.totalTransCurrentSessionSale = totalTransCurrentSessionSale;
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
    }

    setChartData(items): void {
        let sales = [];
        let trans = [];

        _.forEach(items, (item) => {

            sales.push({
                'name': item['period'],
                'value': item['sales']
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
        this._dashboardService
            .getSalesSummary(datePeriod)
            .subscribe(result => {
                this.init(result.salesSummary);
                // this.setChartData(result.salesSummary);
                // this.hideLoading();
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

    appSalesSummaryDateInterval = AppSalesSummaryDatePeriod;
    selectedSalesSummaryDatePeriod: AppSalesSummaryDatePeriod = AppSalesSummaryDatePeriod.Daily;

    dashboardHeaderStats: DashboardHeaderStats;
    salesSummaryChart: SalesSummaryChart;

    constructor(
        injector: Injector,
        private _dashboardService: DashboardServiceProxy
    ) {
        super(injector);
       // this.dashboardHeaderStats = new DashboardHeaderStats();
        //this.salesSummaryChart = new SalesSummaryChart(this._dashboardService, 'salesStatistics');
    }

    getDashboardStatisticsData(datePeriod): void {
        this.salesSummaryChart.showLoading();

        this._dashboardService
            .getDashboardData(datePeriod)
            .subscribe(result => {
                this.dashboardHeaderStats.init(result.totalTransSale, result.totalTransToday, result.totalTransCurrentSession, result.totalTransCurrentSessionSale);
                this.salesSummaryChart.init(result.salesSummary);
            });
    }

    ngAfterViewInit(): void {
       // this.getDashboardStatisticsData(AppSalesSummaryDatePeriod.Daily);
       // this.sessionStatsTable.init();
    }
}



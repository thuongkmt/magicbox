import { AfterViewInit, Component, Injector, ViewEncapsulation } from '@angular/core';
import { AppSalesSummaryDatePeriod } from '@shared/AppEnums';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { DashboardServiceProxy } from '@shared/service-proxies/dashboard-service-proxies';
import { curveBasis } from 'd3-shape';
import { MessageSignalrService } from '../../shared/common/signalr/message-signalr.service';
import { SignalRHelper } from 'shared/helpers/SignalRHelper';
import { ISubscription } from "rxjs/Subscription";
import { ParentChildService } from '../../shared/common/parent-child/parent-child.service' 
import * as _ from 'lodash';
import { DraggableItemService } from 'ngx-bootstrap';
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
        this.totalSales = 0
        this.totalTrans = 0
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

        this.showLoading();
        this._dashboardService
            .getSalesSummary(datePeriod)
            .subscribe(result => {
                this.init(result.salesSummary)
                // this.setChartData(result.salesSummary);
                // this.hideLoading();
            });
    }
}

class SessionStatsTable extends DashboardChartBase {
    stats: Array<any>;
    colors = ['#00c5dc', '#f4516c', '#34bfa3', '#ffb822'];
    customColors = [
        { name: '1', value: '#00c5dc' },
        { name: '2', value: '#f4516c' },
        { name: '3', value: '#34bfa3' },
        { name: '4', value: '#ffb822' },
        { name: '5', value: '#00c5dc' }
    ];

    curve: any = curveBasis;

    constructor(private _dashboardService: DashboardServiceProxy) {
        super();
    }

    init() {
        this.reload();
    }

    formatData(): any {
        for (let j = 0; j < this.stats.length; j++) {
            let stat = this.stats[j];

            let series = [];
            for (let i = 0; i < stat.change.length; i++) {
                series.push({
                    name: i + 1,
                    value: stat.change[i]
                });
            }

            stat.changeData = [
                {
                    'name': j + 1,
                    'series': series
                }
            ];

        }
    }

    reload() {
        this.showLoading();
        this._dashboardService
            .getSessionStats()
            .subscribe(result => {
                this.stats = result.sessionStat;
               // this.formatData();
                this.hideLoading();
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
    sessionStatsTable: SessionStatsTable;
    private onDetectDishFromTable: ISubscription
    private comParentChildService: ParentChildService
    items = [
        "item1",
        "item1",
      ];


    constructor(
        injector: Injector,
        private _dashboardService: DashboardServiceProxy,
        private _messageSignalrService: MessageSignalrService,
    ) {
        super(injector);
        this.dashboardHeaderStats = new DashboardHeaderStats();
        this.salesSummaryChart = new SalesSummaryChart(this._dashboardService, 'salesStatistics');
        this.sessionStatsTable = new SessionStatsTable(this._dashboardService);
    }

    ngOnInit(): void {
        if (this.appSession.application) {
            if (!this._messageSignalrService.isMessageConnected) {
                SignalRHelper.initSignalR(() => {
                    this._messageSignalrService.init();
                });
            }
        }
        if (!this.onDetectDishFromTable) {
            this.onDetectDishFromTable = this.comParentChildService.on('app.message.messageReceived').subscribe($data => this.onMessageReceived($data));
        }
    }

    ngOnDestroy() {
        if (this.onDetectDishFromTable) this.onDetectDishFromTable.unsubscribe()
    }


    onMessageReceived(message) {
        let mess = JSON.parse(message.message);

        console.log(mess.message);
        console.log('aaaa');
        if(this.items.length>=50) this.items=[];
        this.items.push(mess.message);
        //abp.utils.formatString('{0}: {1}', 'aaaaaaaaaaaaa', abp.utils.truncateString(message.message, 100));
    };


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
        this.getDashboardStatisticsData(AppSalesSummaryDatePeriod.Daily);
        this.sessionStatsTable.init();
    }
}



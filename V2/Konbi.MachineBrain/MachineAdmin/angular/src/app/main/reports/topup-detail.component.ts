import { Component, OnInit, Injector, ViewEncapsulation, ViewChild, Inject, Optional, } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TopupReportServiceProxy } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy, API_BASE_URL } from '@shared/service-proxies/service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { ModalDirective } from 'ngx-bootstrap';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';

@Component({
    selector: 'detail-topup',
    templateUrl: './topup-detail.component.html',
    styleUrls: ['./topup-detail.component.less'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]

})
export class TopupDetailComponent extends AppComponentBase implements OnInit {
    @ViewChild('detailTopupModal') detailTopupModal: ModalDirective;
    // @ViewChild('dataTable') dataTable: Table;
    // @ViewChild('paginator') paginator: Paginator;

    constructor(injector: Injector,
        private topupReportService: TopupReportServiceProxy) {
        super(injector);
    }

    ngOnInit(): void { }

    loadDetailList = (topupId?: string) => {
        this.primengTableHelper.showLoadingIndicator();
        this.topupReportService.getDetail(topupId)
            .subscribe(result => {
                this.primengTableHelper.totalRecordsCount = result.items.length;// result.totalCount;
                this.primengTableHelper.records = result.items;
                this.primengTableHelper.hideLoadingIndicator();
            });
    }

    show(topupId?: string): void {
        this.loadDetailList(topupId);
        this.detailTopupModal.show();
    }

    close = () => {
        this.detailTopupModal.hide();
    }
}

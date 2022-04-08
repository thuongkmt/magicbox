import { Component, OnInit, Injector, ViewEncapsulation, ViewChild, Inject, Optional, } from '@angular/core';
import { TopupReportServiceProxy, TopupDetailDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { ModalDirective } from 'ngx-bootstrap';
import * as _ from 'lodash';
import { PrimengTableHelper } from 'shared/helpers/PrimengTableHelper';

@Component({
    selector: 'detail-topup',
    templateUrl: './topup-detail.component.html',
    styleUrls: ['./topup-detail.component.less'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]

})
export class TopupDetailComponent extends AppComponentBase implements OnInit {
    @ViewChild('detailTopupModal') detailTopupModal: ModalDirective;
    throwOutProducts: PrimengTableHelper;
    newProducts: PrimengTableHelper;

    constructor(injector: Injector,
        private topupReportService: TopupReportServiceProxy) {
        super(injector);
        this.throwOutProducts = new PrimengTableHelper();
        this.newProducts = new PrimengTableHelper();
    }

    ngOnInit(): void { }

    loadDetailList = (topupId?: string) => {
        this.primengTableHelper.showLoadingIndicator();
        this.topupReportService.getDetail(topupId)
            .subscribe(result => {
                if (result.items != null) {
                    this.primengTableHelper.totalRecordsCount = result.items.filter(x => x.type == 2).length;
                    this.primengTableHelper.records = result.items.filter(x => x.type == 2);
                    this.primengTableHelper.hideLoadingIndicator();

                    this.newProducts.totalRecordsCount = result.items.filter(x => x.type == 1).length;
                    this.newProducts.records = result.items.filter(x => x.type == 1);
                    this.newProducts.hideLoadingIndicator();

                    this.throwOutProducts.totalRecordsCount = result.items.filter(x => x.type == 3).length;
                    this.throwOutProducts.records = result.items.filter(x => x.type == 3);
                    this.throwOutProducts.hideLoadingIndicator();
                }
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

import { Injector, ElementRef, Component, ViewChild, OnInit, AfterViewInit } from '@angular/core';
import { AppConsts } from '@shared/AppConsts';
import { EditionPaymentType, SubscriptionStartType } from '@shared/AppEnums';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TenantLoginInfoDto } from '@shared/service-proxies/service-proxies';
import * as moment from 'moment';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { LayoutRefService } from '@metronic/app/core/services/layout/layout-ref.service';
import { UrlHelper } from '@shared/helpers/UrlHelper';

@Component({
    templateUrl: './default-layout.component.html',
    selector: 'default-layout',
    animations: [appModuleAnimation()]
})
export class DefaultLayoutComponent extends AppComponentBase implements OnInit, AfterViewInit {

    @ViewChild('mHeader') mHeader: ElementRef;

    tenant: TenantLoginInfoDto = new TenantLoginInfoDto();
    subscriptionStartType = SubscriptionStartType;
    editionPaymentType: typeof EditionPaymentType = EditionPaymentType;
    installationMode = true;

    constructor(
        injector: Injector,
        private layoutRefService: LayoutRefService
    ) {
        super(injector);
    }

    ngOnInit() {
        this.installationMode = UrlHelper.isInstallUrl(location.href);
    }

    ngAfterViewInit(): void {
        this.layoutRefService.addElement('header', this.mHeader.nativeElement);
    }

    subscriptionStatusBarVisible(): boolean {
        return this.appSession.tenantId > 0 && (this.appSession.tenant.isInTrialPeriod || this.subscriptionIsExpiringSoon());
    }

    subscriptionIsExpiringSoon(): boolean {
        if (this.appSession.tenant.subscriptionEndDateUtc) {
            return moment().utc().add(AppConsts.subscriptionExpireNootifyDayCount, 'days') >= moment(this.appSession.tenant.subscriptionEndDateUtc);
        }

        return false;
    }

    getSubscriptionExpiringDayCount(): number {
        if (!this.appSession.tenant.subscriptionEndDateUtc) {
            return 0;
        }

        return Math.round(moment.utc(this.appSession.tenant.subscriptionEndDateUtc).diff(moment().utc(), 'days', true));
    }

    getTrialSubscriptionNotification(): string {
        return this.l('TrialSubscriptionNotification',
            '<strong>' + this.appSession.tenant.edition.displayName + '</strong>',
            '<a href=\'/account/buy?editionId=' + this.appSession.tenant.edition.id + '&editionPaymentType=' + this.editionPaymentType.BuyNow + '\'>' + this.l('ClickHere') + '</a>');
    }

    getExpireNotification(localizationKey: string): string {
        return this.l(localizationKey, this.getSubscriptionExpiringDayCount());
    }
}

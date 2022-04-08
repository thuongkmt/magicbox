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
    templateUrl: './demo8-layout.component.html',
    selector: 'demo8-layout',
    animations: [appModuleAnimation()]
})
export class Demo8LayoutComponent extends AppComponentBase implements OnInit, AfterViewInit {

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
        this.initStickyHeader();
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

    initStickyHeader(): any {
        let headerEl = mUtil.get('m_header');
        let options = {
            offset: {
                desktop: null,
                mobile: null
            },
            minimize: {
                mobile: null,
                desktop: null
            },
            classic: {
                mobile: true,
                desktop: true
            }
        };

        if (mUtil.attr(headerEl, 'm-minimize-mobile') === 'minimize') {
            options.minimize.mobile = {};
            options.minimize.mobile.on = 'm-header--minimize-on';
            options.minimize.mobile.off = 'm-header--minimize-off';
        } else {
            options.minimize.mobile = false;
        }

        if (mUtil.attr(headerEl, 'm-minimize') === 'minimize') {
            options.minimize.desktop = {};
            options.minimize.desktop.on = 'm-header--minimize-on';
            options.minimize.desktop.off = 'm-header--minimize-off';
        } else {
            options.minimize.desktop = false;
        }

        if (mUtil.attr(headerEl, 'm-minimize-offset')) {
            options.offset.desktop = mUtil.attr(headerEl, 'm-minimize-offset');
        }

        if (mUtil.attr(headerEl, 'm-minimize-mobile-offset')) {
            options.offset.mobile = mUtil.attr(headerEl, 'm-minimize-mobile-offset');
        }

        return new mHeader('m_header', options);
    }
}

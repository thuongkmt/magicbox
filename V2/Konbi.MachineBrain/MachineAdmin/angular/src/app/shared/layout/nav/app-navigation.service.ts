import { PermissionCheckerService } from '@abp/auth/permission-checker.service';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { FeatureCheckerService } from '@abp/features/feature-checker.service';
import { Injector, Injectable } from '@angular/core';
import { AppMenu } from './app-menu';
import { AppMenuItem } from './app-menu-item';

@Injectable()
export class AppNavigationService {

    feature: FeatureCheckerService;

    constructor(
        injector: Injector,
        private _permissionCheckerService: PermissionCheckerService,
        private _appSessionService: AppSessionService
    ) {
        this.feature = injector.get(FeatureCheckerService);
    }

    getMenu(): AppMenu {
        let appMenu = new AppMenu('MainMenu', 'MainMenu', [

            // new AppMenuItem('Dashboard', 'Pages.Administration.Host.Dashboard', 'flaticon-line-graph', '/app/admin/hostDashboard'),
            // new AppMenuItem('Dashboard', 'Pages.Tenant.Dashboard', 'flaticon-line-graph', '/app/main/dashboard'),
            // new AppMenuItem('Tenants', 'Pages.Tenants', 'flaticon-list-3', '/app/admin/tenants'),
            // new AppMenuItem('Editions', 'Pages.Editions', 'flaticon-app', '/app/admin/editions'),
            // // new AppMenuItem('DemoUiComponents', 'Pages.DemoUiComponents', 'flaticon-shapes', '/app/admin/demo-ui-components')
            new AppMenuItem('Product Manager', 'Pages.Products', 'flaticon-cart', '', [
                new AppMenuItem('Categories', 'Pages.Products', 'flaticon-more', '/app/main/products/productCategories'),
                new AppMenuItem('Products', 'Pages.Products', 'flaticon-background', '/app/main/products/products')
            ]),
        ]);

        appMenu.items.push(
            new AppMenuItem('Topup', 'Pages.Inventories.Topup', 'flaticon-clock-2', '/app/main/inventories/topup')
        );
        appMenu.items.push(
            new AppMenuItem('Restock', 'Pages.Inventories.Restock', 'flaticon-clock-2', '/app/main/inventories/restock'),
            new AppMenuItem('Restock Report', 'Pages.Reports', 'flaticon-statistics', '/app/main/reports/restockReport')
        );

        appMenu.items.push(
            new AppMenuItem('Unload', '', 'flaticon-bag', '/app/main/inventories/unload')
        );

        appMenu.items.push(
            new AppMenuItem('Inventories', 'Pages.Inventories', 'flaticon-list', '', [
                new AppMenuItem('Summary', 'Pages.Inventories', 'flaticon-file-1', '/app/main/inventories/current-by-product'),
                new AppMenuItem('Current', 'Pages.Inventories', 'flaticon-file-1', '/app/main/inventories/current-inventories'),
                new AppMenuItem('Topped up', 'Pages.Inventories', 'flaticon-file-1', '/app/main/inventories/inventories')])
        );

        appMenu.items.push(
            new AppMenuItem('Transactions', 'Pages.Transactions', 'flaticon-list-3', '', [
                new AppMenuItem('Success', 'Pages.Transactions', 'flaticon-signs-1', '/app/main/transactions-success'),
                new AppMenuItem('Not Success', 'Pages.Transactions', 'flaticon-danger', '/app/main/transactions-error'),
                new AppMenuItem('Black list cards','Pages.Transactions','flaticon-layer','/app/main/transactions/black-list-cards')
            ])
        );

        appMenu.items.push(
            new AppMenuItem('Temperature Logs', 'Pages.Temperature', 'flaticon-line-graph', '/app/main/temperature-logs'),
        );

        appMenu.items.push(
            new AppMenuItem('Settings', 'Pages.SystemSetting', 'flaticon-user-settings', '', [
                new AppMenuItem('System Setting', '', 'flaticon-settings', '/app/main/rfidtable-settings'),
                new AppMenuItem('Machine setting', 'Pages.Machine.Setting', 'flaticon-signs-1', '/app/main/device-setting/machine-setting')]),
            new AppMenuItem('Stop sale setting', 'Pages.AlertSetting', 'flaticon-alert', '/app/main/configurations/alert-configurations')
        );

        appMenu.items.push(
            new AppMenuItem('Diagnostic', 'Pages.SystemSetting', 'flaticon-laptop', '', [
                new AppMenuItem('Unstable Tags', 'Pages.SystemSetting', 'flaticon-laptop', '/app/main/diagnostic/unstable-tags'),
                new AppMenuItem('Hardware', 'Pages.SystemSetting', 'flaticon-laptop', '/app/main/diagnostic/hardware')
            ]));

        appMenu.items.push
            (
                 new AppMenuItem('UserCredits', 'Pages.UserCredits', 'flaticon-more', '/app/main/credit/userCredits'),
                 new AppMenuItem('CreditHistories', 'Pages.UserCredits', 'flaticon-more', '/app/main/credit/creditHistories'),
               
         //   new AppMenuItem('RestockSessions', 'Pages.RestockSessions', 'flaticon-more', '/app/main/restock/restockSessions'),
             new AppMenuItem('Administration', '', 'flaticon-interface-8', '', [
                    new AppMenuItem('OrganizationUnits', 'Pages.Administration.OrganizationUnits', 'flaticon-map', '/app/admin/organization-units'),
                    new AppMenuItem('Roles', 'Pages.Administration.Roles', 'flaticon-suitcase', '/app/admin/roles'),
                    new AppMenuItem('Users', 'Pages.Administration.Users', 'flaticon-users', '/app/admin/users'),
                    new AppMenuItem('Languages', 'Pages.Administration.Languages', 'flaticon-tabs', '/app/admin/languages'),
                    new AppMenuItem('AuditLogs', 'Pages.Administration.AuditLogs', 'flaticon-folder-1', '/app/admin/auditLogs'),
                    new AppMenuItem('Maintenance', 'Pages.Administration.Host.Maintenance', 'flaticon-lock', '/app/admin/maintenance'),
                    new AppMenuItem('Subscription', 'Pages.Administration.Tenant.SubscriptionManagement', 'flaticon-refresh', '/app/admin/subscription-management'),
                    new AppMenuItem('VisualSettings', 'Pages.Administration.UiCustomization', 'flaticon-medical', '/app/admin/ui-customization'),
                    new AppMenuItem('Settings', 'Pages.Administration.Host.Settings', 'flaticon-settings', '/app/admin/hostSettings'),
                    new AppMenuItem('Settings', 'Pages.Administration.Tenant.Settings', 'flaticon-settings', '/app/admin/tenantSettings'),
                    // new AppMenuItem('SyncSettings', 'Pages.Administration.SyncSetting', 'flaticon-settings', '/app/admin/syncsetting'),
                ]));

        return appMenu;
    }

    checkChildMenuItemPermission(menuItem): boolean {

        for (let i = 0; i < menuItem.items.length; i++) {
            let subMenuItem = menuItem.items[i];

            if (subMenuItem.permissionName) {
                return this._permissionCheckerService.isGranted(subMenuItem.permissionName);
            } else if (subMenuItem.items && subMenuItem.items.length) {
                return this.checkChildMenuItemPermission(subMenuItem);
            }
            return true;
        }

        return false;
    }

    showMenuItem(menuItem: AppMenuItem): boolean {
        if (menuItem.permissionName === 'Pages.Administration.Tenant.SubscriptionManagement' && this._appSessionService.tenant && !this._appSessionService.tenant.edition) {
            return false;
        }

        let hideMenuItem = false;

        if (menuItem.requiresAuthentication && !this._appSessionService.user) {
            hideMenuItem = true;
        }

        if (menuItem.permissionName && !this._permissionCheckerService.isGranted(menuItem.permissionName)) {
            hideMenuItem = true;
        }

        if (menuItem.hasFeatureDependency() && !menuItem.featureDependencySatisfied()) {
            hideMenuItem = true;
        }

        if (!hideMenuItem && menuItem.items && menuItem.items.length) {
            return this.checkChildMenuItemPermission(menuItem);
        }

        return !hideMenuItem;
    }

    getRfidTableEabled(): boolean {
        return (!this._appSessionService.tenantId || this.feature.isEnabled('App.RFIDTableFeature'));
    }
}

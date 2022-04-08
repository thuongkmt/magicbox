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
        var appMenu = new AppMenu('MainMenu', 'MainMenu', [
            new AppMenuItem('Dashboard', 'Pages.Tenant.Dashboard', 'flaticon-line-graph', '/app/main/dashboard'),            
            new AppMenuItem('Machine Manager', 'Pages.Machines', 'flaticon-interface-8', '',
            [
                new AppMenuItem('Configure Machines', 'Pages.Machines', 'flaticon-users', '/app/main/machinesconfig'),
                new AppMenuItem('Machine Status', 'Pages.Machines', 'flaticon-users', '/app/main/machinestatuses'),
            ]),
             new AppMenuItem('Tenants', 'Pages.Tenants', 'flaticon-list-3', '/app/admin/tenants'),
             //new AppMenuItem('Editions', 'Pages.Editions', 'flaticon-app', '/app/admin/editions'),
            //new AppMenuItem('Getting Started', 'Pages.Tenant.Dashboard', 'flaticon-shapes', '/app/main/getting-started'),
            new AppMenuItem('Product Manager', 'Pages.Products', 'flaticon-cart', '',[
                new AppMenuItem('Categories', 'Pages.Products', 'flaticon-more', '/app/main/products/productCategories'),
                new AppMenuItem('Products', 'Pages.Products', 'flaticon-background', '/app/main/products/products'),
                new AppMenuItem('Product Prices', 'Pages.Products', 'flaticon-interface-11', '/app/main/products/product-machine')
            ]),
            new AppMenuItem('Inventory Overview', 'Pages.Inventories', 'flaticon-tool', '',[
                new AppMenuItem('Mapped Product Tags', 'Pages.Inventories', 'flaticon-interface-9', '/app/main/products/product-tags'),
                new AppMenuItem('Inventory Overview', 'Pages.Inventories', 'flaticon-settings', '/app/main/inventories-overview-realtime/inventories-overview-realtime'),
                new AppMenuItem('Current Inventory', 'Pages.Inventories', 'flaticon-interface-3', '/app/main/inventories/current-inventories'),
            ]),
            
            new AppMenuItem('Restock Report', 'Pages.Reports', 'flaticon-statistics', '/app/main/reports/restockReport'),
           //new AppMenuItem('RestockSessions', 'Pages.RestockSessions', 'flaticon-statistics', '/app/main/restock/restockSessions'),
        ]);

        appMenu.items.push(
            new AppMenuItem('MagicboxTransactions', 'Pages.Transactions', 'flaticon-list-3', '', [
                new AppMenuItem('Success', 'Pages.Transactions', 'flaticon-signs-1', '/app/main/magicbox-transactions-success'),
                new AppMenuItem('Error', 'Pages.Transactions', 'flaticon-danger', '/app/main/magicbox-transactions-error')
            ])
        ),
        appMenu.items.push(
            new AppMenuItem('Temperature Logs', '', 'flaticon-line-graph', '/app/main/temperature-logs'),
        ),
        appMenu.items.push(
            new AppMenuItem('KonbiWallet', '', 'flaticon-line-graph', '/app/main/konbi-wallet'),
        ),
        appMenu.items.push(
            new AppMenuItem('Settings', '', 'flaticon-settings', '', [
                new AppMenuItem('System Setting', 'Pages.SystemSetting', 'flaticon-computer', '/app/main/system-settings')
            ])
        );

        appMenu.items.push(
            
             
           
                new AppMenuItem('Administration', '', 'flaticon-computer', '', [
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
                new AppMenuItem('Restocker', '', 'flaticon-line-graph', '/app/main/restocker')
            ])
            
            );

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

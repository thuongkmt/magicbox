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


            new AppMenuItem('Dashboard', 'Pages.Administration', 'flaticon-line-graph', '/app/admin/hostDashboard'),
             //migrate from old cloud
             new AppMenuItem('Machines', '', 'flaticon-interface-8', '', [
                // new AppMenuItem('Machine Status', 'Pages.Administration', 'flaticon-map', '/app/admin/machinestatuses'),
                // new AppMenuItem('VMC Diagnotic', 'Pages.Administration', 'flaticon-suitcase', '/app/admin/roles'),
                new AppMenuItem('Status', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Settings', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Setup', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Behaviour Flags', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
            ]),

            new AppMenuItem('Products', '', 'flaticon-interface-8', '', [
                new AppMenuItem('Categories', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Product Details', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Machine Settings', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
            ]),

            new AppMenuItem('Inventory', '', 'flaticon-interface-8', '', [
                new AppMenuItem('Overall Machine Inventory', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Per Machine Per Product', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Restock History', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Expiry Overview', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
            ]),

             
            new AppMenuItem('Uesrs', '', 'flaticon-interface-8', '', [
                new AppMenuItem('Roles Setup', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Menus/Views', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
            ]),
           
            new AppMenuItem('Sales', '', 'flaticon-interface-8', '', [
                new AppMenuItem('Success Transactions', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Error Transactions', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Reports', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
            ]),

            new AppMenuItem('Events', '', 'flaticon-interface-8', '', [
                new AppMenuItem('Machine events', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Error', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Cloud events', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
            ]),


            new AppMenuItem('Alerts', '', 'flaticon-interface-8', '', [
                new AppMenuItem('Define Machine Type Alert', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Machine Alerts Routing', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
                new AppMenuItem('Active/Deactive Individual Alert', 'Pages.Administration', 'flaticon-users', '/app/admin/hostDashboard'),
            ]),

        ]);

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

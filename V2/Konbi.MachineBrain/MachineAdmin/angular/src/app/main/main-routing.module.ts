import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { RestockSessionsComponent } from './restock/restockSessions/restockSessions.component';
import { CreateOrEditRestockSessionComponent } from './restock/restockSessions/create-or-edit-restockSession.component';
import { ViewRestockSessionComponent } from './restock/restockSessions/view-restockSession.component';
import { CreditHistoriesComponent } from './credit/creditHistories/creditHistories.component';
import { UserCreditsComponent } from './credit/userCredits/userCredits.component';
import { InventoriesComponent } from './inventories/inventories/inventories.component';
import { ProductsComponent } from './products/products/products.component';
import { SessionsComponent } from './machines/sessions/sessions.component';
import { DiscsComponent } from './plate/discs/discs.component';
import { PlatesComponent } from './plate/plates/plates.component';
import { PlateCategoriesComponent } from './plate/plateCategories/plateCategories.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { PlateMenusComponent } from './plate/plate-menus/plate-menus.component';
import { PlateMenuCalendarComponent } from './plate/plate-menu-calendar/plate-menu-calendar.component';
import { TransactionsComponent } from './transactions/transactions.component';
import { RfidTableComponent } from 'customer/rfid-table/rfid-table.component';
import {RfidTableSettingComponent} from './device-setting/rfid-table-setting/rfid-table-setting.component';
import {TraysComponent} from './plate/trays/trays.component';
import { AbpSettingComponent } from './system-setting/abp-setting.component';
import { MdbcashlessDiagnosticComponent } from './device-setting/mdbcashless-diagnostic/mdbcashless-diagnostic.component';
import { TopupComponent } from './inventories/topup/topup.component';
import { CurrentInventoriesComponent } from './inventories/current-inventories/current-inventories.component';
import { CurrentByProductComponent } from './inventories/current-inventories/current-by-product.component';
import { TransactionDetailComponent } from './transactions/transaction-detail/transaction-detail.component';
import { RfidtableSettingComponent } from './system-setting/rfidtable-setting/rfidtable-setting.component';
import { DiagnosticComponent } from './diagnostic/diagnostic.component';
import { ProductCategoriesComponent } from './products/productCategories/productCategories.component';
import { TemperatureLogsComponent } from './temperature-logs/temperature-logs.component';
import { RestockComponent } from './inventories/restock/restock.component';
import { TopupListComponent } from './reports/topup-list.component';
import { AlertConfigurationsComponent } from './configurations/alert-configurations/alert-configurations.component';
import { MachineSettingComponent} from './device-setting/machine-setting/machine-setting.component';
import { UnstableTagDiagnosticComponent } from './diagnostic/unstable-tag-diagnostic/unstable-tag-diagnostic.component';
import { UnloadComponent } from './inventories/unload/unload.component';
import { BlackListCardsComponent } from './transactions/black-list-cards/black-list-cards.component';

@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: '',
                children: [
                    { path: 'restock/restockSessions', component: RestockSessionsComponent, data: { permission: 'Pages.RestockSessions' }  },
                    { path: 'restock/restockSessions/createOrEdit', component: CreateOrEditRestockSessionComponent, data: { permission: 'Pages.RestockSessions.Create' }  },
                    { path: 'restock/restockSessions/view', component: ViewRestockSessionComponent, data: { permission: 'Pages.RestockSessions' }  },
                    { path: 'credit/creditHistories', component: CreditHistoriesComponent, data: { permission: 'Pages.CreditHistories' }  },
                    { path: 'credit/userCredits', component: UserCreditsComponent, data: { permission: 'Pages.UserCredits' }  },
                    { path: 'inventories/topup', component: TopupComponent, data: { permission: 'Pages.Inventories.Topup' }  },
                    { path: 'inventories/restock', component: RestockComponent, data: { permission: 'Pages.Inventories.Restock' }  },
                    { path: 'inventories/unload', component: UnloadComponent, data: { permission: 'Pages.Inventories' }  },
                    { path: 'inventories/inventories', component: InventoriesComponent, data: { permission: 'Pages.Inventories' }  },
                    { path: 'inventories/current-inventories', component: CurrentInventoriesComponent, data: { permission: 'Pages.Inventories' }  },
                    { path: 'inventories/current-by-product', component: CurrentByProductComponent, data: { permission: 'Pages.Inventories' }  },
                    { path: 'products/products', component: ProductsComponent, data: { permission: 'Pages.Products' }  },
                    { path: 'products/productCategories', component: ProductCategoriesComponent, data: { permission: 'Pages.ProductCategories' }  },
                    { path: 'machines/sessions', component: SessionsComponent, data: { permission: 'Pages.Sessions' }  },
                    { path: 'plate/dishes', component: DiscsComponent, data: { permission: 'Pages.Discs' }  },
                    { path: 'plate/dishes/:plate_id', component: DiscsComponent, data: { permission: 'Pages.Discs' }  },
                    { path: 'plate/plates', component: PlatesComponent, data: { permission: 'Pages.Plates' }  },
                    { path: 'plate/plateCategories', component: PlateCategoriesComponent, data: { permission: 'Pages.PlateCategories' }  },
                    { path: 'plate/plateMenus', component: PlateMenusComponent, data: { permission: 'Pages.PlateMenus' }  },
                    { path: 'plate/plateMenus/:datefilter', component: PlateMenusComponent, data: { permission: 'Pages.PlateMenus' }  },
                    { path: 'plate/plateMenus/:datefilter/:sessionfilter', component: PlateMenusComponent, data: { permission: 'Pages.PlateMenus' }  },
                    { path: 'plate/plateMenuCalendar', component: PlateMenuCalendarComponent, data: { permission: 'Pages.PlateMenus' }  },
                    { path: 'plate/trays', component: TraysComponent, data: { permission: 'Pages.Trays' }  },
                    { path: 'dashboard', component: DashboardComponent, data: { permission: 'Pages.Tenant.Dashboard' } },
                    { path: 'temperature-logs', component: TemperatureLogsComponent, data: { permission: 'Pages.Temperature' } },
                    { path: 'transactions-success', component: TransactionsComponent, data: { permission: 'Pages.Transactions' } },
                    { path: 'transactions-error', component: TransactionsComponent, data: { permission: 'Pages.Transactions' } },
                    { path: 'devicesetting/rfid-table', component: RfidTableSettingComponent, data: { permission: 'Pages.DeviceSetting' } },
                    { path: 'devicesetting/mdbcashless-diagnostic', component: MdbcashlessDiagnosticComponent, data: { permission: 'Pages.DeviceSetting' } },
                    { path: 'system-settings', component: AbpSettingComponent, data: { permission: 'Pages.SystemSetting' } },
                    { path: 'rfidtable-settings', component: RfidtableSettingComponent, data: { permission: 'Pages.SystemSetting' } },
                    { path: 'transactions-detail', component: TransactionDetailComponent, data: { permission: 'Pages.Transactions' } },
                    { path: 'diagnostic/hardware', component: DiagnosticComponent, data: { permission: 'Pages.SystemSetting' } },
                    { path: 'diagnostic/unstable-tags', component: UnstableTagDiagnosticComponent, data: { permission: 'Pages.SystemSetting' } },
                    { path: 'reports/restockReport', component: TopupListComponent, data: { permission: 'Pages.Reports' } },
                    { path: 'configurations/alert-configurations', component: AlertConfigurationsComponent, data: { permission: 'Pages.AlertSetting' } },
                    { path: 'device-setting/machine-setting', component: MachineSettingComponent, data: {permission: 'Pages.Machine.Setting'}},
                    { path: 'transactions/black-list-cards', component: BlackListCardsComponent, data: {permission: 'Pages.Transactions'}}
                ]
            }
        ]),
    ],
    exports: [
        RouterModule
    ]
})
export class MainRoutingModule { }

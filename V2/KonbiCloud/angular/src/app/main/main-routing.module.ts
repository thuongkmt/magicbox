import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { RestockSessionsComponent } from './restock/restockSessions/restockSessions.component';
import { InventoriesComponent } from './inventories/inventories.component';
import { CurrentInventoriesComponent } from './inventories/current-inventories/current-inventories.component';
import { InventoryDetailComponent } from './inventories/inventory-detail.component';
import { TopupListComponent } from './reports/topup-list.component';
import { TopupDetailComponent } from './reports/topup-detail.component';
import { ProductsComponent } from './products/products/products.component';
import { ProductCategoriesComponent } from './products/productCategories/productCategories.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { TransactionsComponent } from './transactions/transactions.component';
import { AbpSettingComponent } from './system-setting/abp-setting.component';
import { GetStartedComponent } from './get-started/get-started.component';
import { MagicboxTransactionComponent } from './magicbox-transaction/magicbox-transaction.component';
import { TemperatureLogsComponent } from './temperature-logs/temperature-logs.component';
import { ProductTagsComponent } from './products/product-tags/product-tags.component';
import { ProductsMachineComponent } from './products/products-machine/products-machine.component';
import { InventoriesOverviewRealtimeComponent} from './inventories-overview-realtime/inventories-overview-realtime.component';
import { InventoriesDetailRealTimeComponent} from './inventories-detail-real-time/inventories-detail-real-time.component';
import { AlertConfigurationsComponent } from './configurations/alert-configurations/alert-configurations.component';
import { MachinesComponent} from './machines/machines.component';
import { MachineStatusesComponent} from './machines/machine-statuses/machine-statuses.component';
import { ShowDevicesComponent } from "./machines/show-devices/show-devices.component";

import { MachineLoadoutComponent } from './machines/machine-loadout/machine-loadout.component';
import { UninitializedMachineComponent } from './machines/uninitialized-machine/uninitialized-machine.component';
import { RestockerComponent } from './restocker/restocker.component';
import { WooWalletCustomerComponent } from './woo-wallet-customer/woo-wallet-customer.component';

@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: '',
                children: [
                    { path: 'restock/restockSessions', component: RestockSessionsComponent, data: { permission: 'Pages.RestockSessions' }  },
                    { path: 'inventories/inventories', component: InventoriesComponent, data: { permission: 'Pages.Inventories' }  },
                    { path: 'inventories/current-inventories', component: CurrentInventoriesComponent, data: { permission: 'Pages.Inventories' }  },
                    { path: 'inventories/inventoryDetail/:machineId/:topupId', component: InventoryDetailComponent, data: { permission: 'Pages.Inventories' }  },
                    { path: 'products/products', component: ProductsComponent, data: { permission: 'Pages.Products' }  },
                    { path: 'products/productCategories', component: ProductCategoriesComponent, data: { permission: 'Pages.ProductCategories' }  },
                    { path: 'products/product-machine', component: ProductsMachineComponent, data: { permission: 'Pages.ProductsMachinePrice' }  },
                    { path: 'reports/restockReport', component: TopupListComponent, data: { permission: 'Pages.Reports' }  },
                    { path: 'reports/topup-detail/:machineId/:topupId', component: TopupDetailComponent, data: { permission: 'Pages.Reports' }  },
                    { path: 'dashboard', component: DashboardComponent, data: { permission: 'Pages.Tenant.Dashboard' } },
                    { path: 'temperature-logs', component: TemperatureLogsComponent, data: { permission: 'Pages.Machines' } },
                    { path: 'getting-started', component: GetStartedComponent, data: { permission: 'Pages.Tenant.Dashboard' } },
                    { path: 'transactions-success', component: TransactionsComponent, data: { permission: 'Pages.Transactions' } },
                    { path: 'transactions-error', component: TransactionsComponent, data: { permission: 'Pages.Transactions' } },
                    { path: 'system-settings', component: AbpSettingComponent, data: { permission: 'Pages.SystemSetting' } },
                    { path: 'magicbox-transactions-success', component: MagicboxTransactionComponent, data: { permission: 'Pages.Transactions' } },
                    { path: 'magicbox-transactions-error', component: MagicboxTransactionComponent, data: { permission: 'Pages.Transactions' } },
                    { path: 'products/product-tags', component: ProductTagsComponent, data: { permission: 'Pages.Products' }  },
                    { path: 'inventories-overview-realtime/inventories-overview-realtime', component: InventoriesOverviewRealtimeComponent, data: { permission: 'Pages.Inventories' }},
                    { path: 'inventories-detail-real-time/inventories-detail-real-time/:machineId/:topupId', component: InventoriesDetailRealTimeComponent, data: { permission: 'Pages.Inventories' }},
                    { path: 'configurations/alert-configurations', component: AlertConfigurationsComponent, data: { permission: 'Pages.AlertSetting' } },
                    //migrate from old cloud
                    { path: 'machinestatuses', component: MachineStatusesComponent, data: { permission: 'Pages.Machines' } },
                    { path: 'machinesconfig', component: MachinesComponent, data: { permission: 'Pages.Machines' } },
                    { path: 'uninitialized-machines', component: UninitializedMachineComponent, data: { permission: 'Pages.Machines' } },
                    { path: 'ShowDevices/:machineId/:machineName', component: ShowDevicesComponent, data: { permission: 'Pages.Machines' } },
                    { path: 'machine-loadout/:id', component: MachineLoadoutComponent, data: { permission: 'Pages.Machines' }},
                    { path: 'restocker', component: RestockerComponent, data: { permission: 'Pages.Machines' }},
                    { path: 'konbi-wallet', component: WooWalletCustomerComponent, data: { permission: 'Pages.Machines' } },
                ]
            }
        ])
    ],
    exports: [
        RouterModule
    ]
})
export class MainRoutingModule { }

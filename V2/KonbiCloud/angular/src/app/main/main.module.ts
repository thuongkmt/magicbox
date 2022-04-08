import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppCommonModule } from '@app/shared/common/app-common.module';
import { RestockSessionsComponent } from './restock/restockSessions/restockSessions.component';
import { ViewRestockSessionModalComponent } from './restock/restockSessions/view-restockSession-modal.component';
import { CreateOrEditRestockSessionModalComponent } from './restock/restockSessions/create-or-edit-restockSession-modal.component';

import { InventoriesComponent } from './inventories/inventories.component';
import { CurrentInventoriesComponent } from './inventories/current-inventories/current-inventories.component';
import { InventoryDetailComponent } from './inventories/inventory-detail.component';
import { ViewInventoryModalComponent } from './inventories/inventories/view-inventory-modal.component';
import { CreateOrEditInventoryModalComponent } from './inventories/inventories/create-or-edit-inventory-modal.component';
import { ProductLookupTableModalComponent } from './inventories/inventories/product-lookup-table-modal.component';

import { ProductsComponent } from './products/products/products.component';
import { TopupListComponent } from './reports/topup-list.component';
import { TopupDetailComponent } from './reports/topup-detail.component';
import { ViewProductModalComponent } from './products/products/view-product-modal.component';
import { CreateOrEditProductModalComponent } from './products/products/create-or-edit-product-modal.component';
import { ProductCategoryLookupTableModalComponent } from './products/products/productCategory-lookup-table-modal.component';

import { ProductCategoriesComponent } from './products/productCategories/productCategories.component';
import { ViewProductCategoryModalComponent } from './products/productCategories/view-productCategory-modal.component';
import { CreateOrEditProductCategoryModalComponent } from './products/productCategories/create-or-edit-productCategory-modal.component';

import { AutoCompleteModule } from 'primeng/primeng';
import { PaginatorModule } from 'primeng/primeng';
import { EditorModule } from 'primeng/primeng';
import { InputMaskModule } from 'primeng/primeng'; import { FileUploadModule } from 'primeng/primeng';
import { TableModule } from 'primeng/table';

import { UtilsModule } from '@shared/utils/utils.module';
import { CountoModule } from 'angular2-counto';
import { ModalModule, TabsModule, TooltipModule, BsDropdownModule } from 'ngx-bootstrap';
import { DashboardComponent } from './dashboard/dashboard.component';
import { MainRoutingModule } from './main-routing.module';
import { NgxChartsModule } from '@swimlane/ngx-charts';

import { BsDatepickerModule, BsDatepickerConfig, BsDaterangepickerConfig, BsLocaleService } from 'ngx-bootstrap/datepicker';
import { NgxBootstrapDatePickerConfigService } from 'assets/ngx-bootstrap/ngx-bootstrap-datepicker-config.service';
import { TransactionsComponent } from './transactions/transactions.component';
import { TransactionDetailComponent } from './transactions/transaction-detail/transaction-detail.component';
import { PapaParseModule } from 'ngx-papaparse';
import { ngfModule, ngf } from "angular-file";
import { AbpSettingComponent } from './system-setting/abp-setting.component'

import { MachineStatusesComponent } from '../main/machines/machine-statuses/machine-statuses.component';
import { MachinesComponent } from '../main/machines/machines.component';
import { CreateMachineComponent } from '../main/machines/create-machine/create-machine.component';
import { EditMachineComponent } from '../main/machines/edit-machine/edit-machine.component';
import { RemoteSettingsComponent } from '../main/machines/remote-settings/remote-settings.component';

NgxBootstrapDatePickerConfigService.registerNgxBootstrapDatePickerLocales();

//add calendar
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import { GetStartedComponent } from './get-started/get-started.component';
import { MagicboxTransactionComponent } from './magicbox-transaction/magicbox-transaction.component';
import { MtDetailComponent } from './magicbox-transaction/mt-detail/mt-detail.component';
import { ImagePreloadDirective } from './../shared/directives/image-preload.directive';
import { FileUploadComponent } from './../shared/upload/file-upload.component';
import { ResourceManagementComponent } from './../shared/resource-management/resource-management.component';
import { TemperatureLogsComponent } from './temperature-logs/temperature-logs.component';
import { NgMultiSelectDropDownModule } from 'ng-multiselect-dropdown';
import { ProductTagsComponent } from './products/product-tags/product-tags.component';
import { ProductsMachineComponent } from './products/products-machine/products-machine.component';
import { FilterPipe } from './filter.pipe';
import { FilterMachineNamePipe } from './filter.pipe'
import { InventoriesOverviewRealtimeComponent } from './inventories-overview-realtime/inventories-overview-realtime.component';
import { InventoriesDetailRealTimeComponent } from './inventories-detail-real-time/inventories-detail-real-time.component';
import { AlertConfigurationsComponent } from './configurations/alert-configurations/alert-configurations.component';
import { MachineLoadoutComponent } from './machines/machine-loadout/machine-loadout.component';
import { ChangeProductModal } from './machines/machine-loadout/change-product-modal.component';
import { EditLoadoutModal } from './machines/machine-loadout/edit-loadout-modal.component';
import { EditLoadoutItemModal } from './machines/machine-loadout/edit-loadoutitem-modal.component';
import { ShowDevicesComponent } from './machines/show-devices/show-devices.component';
import { DeviceDetailComponent } from './machines/show-devices/device-detail/device-detail.component';
import { UninitializedMachineComponent } from './machines/uninitialized-machine/uninitialized-machine.component';
import { ImportProductModelComponent } from './products/import-product-model/import-product-model.component';
import { RestockerComponent } from './restocker/restocker.component';
import { EditRestockerComponent } from './restocker/edit-restocker/edit-restocker.component';
import { WooWalletCustomerComponent } from './woo-wallet-customer/woo-wallet-customer.component';
import { TransactionWalletComponent } from './woo-wallet-customer/transaction-wallet/transaction-wallet.component';;
import { MapTagByRanageComponent } from './products/product-tags/map-tag-by-ranage/map-tag-by-ranage.component'


@NgModule({
  imports: [
    FileUploadModule,
    AutoCompleteModule,
    PaginatorModule,
    EditorModule,
    InputMaskModule,
    TableModule,
    CommonModule,
    FormsModule,
    ModalModule,
    TabsModule,
    TooltipModule,
    AppCommonModule,
    UtilsModule,
    MainRoutingModule,
    CountoModule,
    NgxChartsModule,
    BsDatepickerModule.forRoot(),
    BsDropdownModule.forRoot(),
    PapaParseModule,
    ngfModule,
    CalendarModule.forRoot({
      provide: DateAdapter,
      useFactory: adapterFactory
    }),
    NgMultiSelectDropDownModule.forRoot()
  ],
  declarations: [
		RestockSessionsComponent,

		ViewRestockSessionModalComponent,
		CreateOrEditRestockSessionModalComponent,
    InventoriesComponent,
    CurrentInventoriesComponent,
    TopupListComponent,
    TopupDetailComponent,
    InventoryDetailComponent,
    ViewInventoryModalComponent, CreateOrEditInventoryModalComponent,
    ProductLookupTableModalComponent,
    ProductsComponent,
    ViewProductModalComponent, CreateOrEditProductModalComponent,
    ProductCategoryLookupTableModalComponent,
    ProductCategoriesComponent,
    ViewProductCategoryModalComponent, CreateOrEditProductCategoryModalComponent,
    DashboardComponent,
    TransactionsComponent, TransactionDetailComponent,
    AbpSettingComponent,
    GetStartedComponent,
    MagicboxTransactionComponent,
    MtDetailComponent,
    ImagePreloadDirective,
    FileUploadComponent,
    ResourceManagementComponent,
    TemperatureLogsComponent,
    ProductTagsComponent,
    ProductsMachineComponent,
    FilterPipe,
    FilterMachineNamePipe,
    InventoriesOverviewRealtimeComponent,
    InventoriesDetailRealTimeComponent,
    AlertConfigurationsComponent,
    //migrate from old cloud
    MachineStatusesComponent,
    MachinesComponent,
    CreateMachineComponent,
    EditMachineComponent,
    RemoteSettingsComponent,
    MachineLoadoutComponent, ChangeProductModal, EditLoadoutModal, EditLoadoutItemModal,
    ShowDevicesComponent,
    DeviceDetailComponent,
    UninitializedMachineComponent,
    ImportProductModelComponent,
    RestockerComponent,
    EditRestockerComponent,
    WooWalletCustomerComponent,
    TransactionWalletComponent,
    MapTagByRanageComponent
  ],
  providers: [
    { provide: BsDatepickerConfig, useFactory: NgxBootstrapDatePickerConfigService.getDatepickerConfig },
    { provide: BsDaterangepickerConfig, useFactory: NgxBootstrapDatePickerConfigService.getDaterangepickerConfig },
    { provide: BsLocaleService, useFactory: NgxBootstrapDatePickerConfigService.getDatepickerLocale }
  ]
})
export class MainModule { }

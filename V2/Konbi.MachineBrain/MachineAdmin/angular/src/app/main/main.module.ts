import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppCommonModule } from '@app/shared/common/app-common.module';
import { RestockSessionsComponent } from './restock/restockSessions/restockSessions.component';
import { ViewRestockSessionComponent } from './restock/restockSessions/view-restockSession.component';
import { CreateOrEditRestockSessionComponent } from './restock/restockSessions/create-or-edit-restockSession.component';

import { CreditHistoriesComponent } from './credit/creditHistories/creditHistories.component';
import { ViewCreditHistoryModalComponent } from './credit/creditHistories/view-creditHistory-modal.component';
import { CreateOrEditCreditHistoryModalComponent } from './credit/creditHistories/create-or-edit-creditHistory-modal.component';
import { UserCreditLookupTableModalComponent } from './credit/creditHistories/userCredit-lookup-table-modal.component';

import { UserCreditsComponent } from './credit/userCredits/userCredits.component';
import { ViewUserCreditModalComponent } from './credit/userCredits/view-userCredit-modal.component';
import { CreateOrEditUserCreditModalComponent } from './credit/userCredits/create-or-edit-userCredit-modal.component';
import { UserLookupTableModalComponent } from './credit/userCredits/user-lookup-table-modal.component';

import { InventoriesComponent } from './inventories/inventories/inventories.component';
import { ViewInventoryModalComponent } from './inventories/inventories/view-inventory-modal.component';
import { CreateOrEditInventoryModalComponent } from './inventories/inventories/create-or-edit-inventory-modal.component';
import { ProductLookupTableModalComponent } from './inventories/inventories/product-lookup-table-modal.component';

import { ProductsComponent } from './products/products/products.component';
import { ViewProductModalComponent } from './products/products/view-product-modal.component';
import { CreateOrEditProductModalComponent } from './products/products/create-or-edit-product-modal.component';

import { ProductCategoriesComponent } from './products/productCategories/productCategories.component';
import { ViewProductCategoryModalComponent } from './products/productCategories/view-productCategory-modal.component';
import { CreateOrEditProductCategoryModalComponent } from './products/productCategories/create-or-edit-productCategory-modal.component';

import { SessionsComponent } from './machines/sessions/sessions.component';
import { ViewSessionModalComponent } from './machines/sessions/view-session-modal.component';
import { CreateOrEditSessionModalComponent } from './machines/sessions/create-or-edit-session-modal.component';

import { DiscsComponent } from './plate/discs/discs.component';
import { CreateOrEditDiscModalComponent } from './plate/discs/create-or-edit-disc-modal.component';
import { PlateLookupTableModalComponent } from './plate/discs/plate-lookup-table-modal.component';

import { PlatesComponent } from './plate/plates/plates.component';
import { ImportPlateModalComponent } from './plate/plates/import-plate-modal.component';
import { CreateOrEditPlateModalComponent } from './plate/plates/create-or-edit-plate-modal.component';
import { PlateCategoryLookupTableModalComponent } from './plate/plates/plateCategory-lookup-table-modal.component';

import { PlateCategoriesComponent } from './plate/plateCategories/plateCategories.component';
//import { ViewPlateCategoryModalComponent } from './plate/plateCategories/view-plateCategory-modal.component';
import { CreateOrEditPlateCategoryModalComponent } from './plate/plateCategories/create-or-edit-plateCategory-modal.component';
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
import { PlateMenusComponent } from './plate/plate-menus/plate-menus.component';
import { TransactionsComponent } from './transactions/transactions.component';
import { TransactionDetailComponent } from './transactions/transaction-detail/transaction-detail.component';

import { PapaParseModule } from 'ngx-papaparse';
import { RfidTableSettingComponent } from './device-setting/rfid-table-setting/rfid-table-setting.component';
import { TraysComponent } from './plate/trays/trays.component';
import { CreateOrEditTrayModalComponent } from './plate/trays/create-or-edit-tray-modal.component';

import { AbpSettingComponent } from './system-setting/abp-setting.component';
import { MdbcashlessDiagnosticComponent } from './device-setting/mdbcashless-diagnostic/mdbcashless-diagnostic.component';
import { ngfModule, ngf } from 'angular-file';
import { ReplicateComponent } from './plate/plate-menus/replicate/replicate.component';
import { ReactiveFormsModule } from '@angular/forms';
import { PlateMenuCalendarComponent } from './plate/plate-menu-calendar/plate-menu-calendar.component';

//add calendar
//import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import { PlateMenuDayComponent } from './plate/plate-menu-calendar/plate-menu-day/plate-menu-day.component';
import { TopupComponent } from './inventories/topup/topup.component';
import { ImportProductModalComponent } from './products/products/import-product-modal.component';
import { RfidtableSettingComponent } from './system-setting/rfidtable-setting/rfidtable-setting.component';
import { EditSettingComponent } from './system-setting/edit-setting/edit-setting.component';
import { CurrentInventoriesComponent } from './inventories/current-inventories/current-inventories.component';
import { DiagnosticComponent } from './diagnostic/diagnostic.component';
import { CurrentByProductComponent } from './inventories/current-inventories/current-by-product.component';


import { ImagePreloadDirective } from './../shared/directives/image-preload.directive';
import { FileUploadComponent } from './../shared/upload/file-upload.component';
import { ResourceManagementComponent } from './../shared/resource-management/resource-management.component';
import { TemperatureLogsComponent } from './temperature-logs/temperature-logs.component';
import { RestockComponent } from './inventories/restock/restock.component';
import { TopupListComponent } from './reports/topup-list.component';
import { TopupDetailComponent } from './reports/topup-detail.component';
import { AlertConfigurationsComponent } from './configurations/alert-configurations/alert-configurations.component';
import { MachineSettingComponent } from './device-setting/machine-setting/machine-setting.component';
import { UnstableTagDiagnosticComponent } from './diagnostic/unstable-tag-diagnostic/unstable-tag-diagnostic.component';
import { UnloadComponent } from './inventories/unload/unload.component';
import { BlackListCardsComponent } from './transactions/black-list-cards/black-list-cards.component';
import { CreateEditBlackListCardComponent } from './transactions/create-edit-black-list-card/create-edit-black-list-card.component';

NgxBootstrapDatePickerConfigService.registerNgxBootstrapDatePickerLocales();

@NgModule({
    imports: [
        FileUploadModule,
        AutoCompleteModule,
        PaginatorModule,
        EditorModule,
        InputMaskModule, TableModule,
        ReactiveFormsModule,
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
        // BrowserAnimationsModule,
        CalendarModule.forRoot({
            provide: DateAdapter,
            useFactory: adapterFactory
        })
    ],
    declarations: [
		RestockSessionsComponent,

		ViewRestockSessionComponent,
		CreateOrEditRestockSessionComponent,
		CreditHistoriesComponent,
        ViewCreditHistoryModalComponent,		
        CreateOrEditCreditHistoryModalComponent,
        UserCreditLookupTableModalComponent,
		UserCreditsComponent,
        ViewUserCreditModalComponent,		
        CreateOrEditUserCreditModalComponent,
        UserLookupTableModalComponent,
		ProductsComponent,
        ViewProductModalComponent,		
        CreateOrEditProductModalComponent,
        InventoriesComponent,
        ViewInventoryModalComponent, 
        CreateOrEditInventoryModalComponent,
        ProductLookupTableModalComponent,
        ProductsComponent,
        ViewProductModalComponent, 
        CreateOrEditProductModalComponent,
        ProductCategoriesComponent,
        ViewProductCategoryModalComponent, 
        CreateOrEditProductCategoryModalComponent,
        ProductsComponent,
        ViewProductModalComponent, 
        CreateOrEditProductModalComponent,
        SessionsComponent,
        ViewSessionModalComponent, 
        CreateOrEditSessionModalComponent,
        DiscsComponent,
        CreateOrEditDiscModalComponent,
        PlateLookupTableModalComponent,
        PlatesComponent,
        ImportPlateModalComponent,
        CreateOrEditPlateModalComponent, //ViewPlateModalComponent,
        PlateCategoryLookupTableModalComponent,
        PlateCategoriesComponent,
        CreateOrEditPlateCategoryModalComponent, //ViewPlateCategoryModalComponent,
        DashboardComponent, PlateMenusComponent,
        TransactionsComponent,
        TransactionDetailComponent,
        RfidTableSettingComponent,
        TraysComponent,
        CreateOrEditTrayModalComponent,
        AbpSettingComponent,
        MdbcashlessDiagnosticComponent,
        ReplicateComponent,
        PlateMenuCalendarComponent,
        PlateMenuDayComponent,
        TopupComponent,
        ImportProductModalComponent,
        RfidtableSettingComponent,
        EditSettingComponent,
        CurrentInventoriesComponent,
        DiagnosticComponent,
        CurrentByProductComponent,
        ImagePreloadDirective,
        FileUploadComponent,
        ResourceManagementComponent,
        TemperatureLogsComponent,
        RestockComponent,
        TopupListComponent,
        TopupDetailComponent,
        AlertConfigurationsComponent,
        MachineSettingComponent,
        UnstableTagDiagnosticComponent,
        UnloadComponent,
        BlackListCardsComponent,
        CreateEditBlackListCardComponent
    ],
    providers: [
        { provide: BsDatepickerConfig, useFactory: NgxBootstrapDatePickerConfigService.getDatepickerConfig },
        { provide: BsDaterangepickerConfig, useFactory: NgxBootstrapDatePickerConfigService.getDaterangepickerConfig },
        { provide: BsLocaleService, useFactory: NgxBootstrapDatePickerConfigService.getDatepickerLocale }
    ]
})
export class MainModule { }

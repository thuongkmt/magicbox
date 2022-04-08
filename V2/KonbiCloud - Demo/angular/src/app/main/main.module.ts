import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppCommonModule } from '@app/shared/common/app-common.module';
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
import { TraysComponent } from './plate/trays/trays.component';
import { CreateOrEditTrayModalComponent } from './plate/trays/create-or-edit-tray-modal.component';
import { ngfModule, ngf } from "angular-file";
import { ReplicateComponent } from './plate/plate-menus/replicate/replicate.component'
import { AbpSettingComponent } from './system-setting/abp-setting.component'

NgxBootstrapDatePickerConfigService.registerNgxBootstrapDatePickerLocales();

//add calendar
import { ReactiveFormsModule } from '@angular/forms';
import { PlateMenuCalendarComponent } from './plate/plate-menu-calendar/plate-menu-calendar.component';
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import { PlateMenuDayComponent } from './plate/plate-menu-calendar/plate-menu-day/plate-menu-day.component';



@NgModule({
    imports: [
        FileUploadModule,
        AutoCompleteModule,
        PaginatorModule,
        EditorModule,
        InputMaskModule, TableModule,

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
        })
    ],
    declarations: [
        SessionsComponent,
        ViewSessionModalComponent, CreateOrEditSessionModalComponent,
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
        TransactionsComponent, TransactionDetailComponent,
        TraysComponent, CreateOrEditTrayModalComponent, ReplicateComponent,
        AbpSettingComponent,
        PlateMenuCalendarComponent,
        PlateMenuDayComponent
    ],
    providers: [
        { provide: BsDatepickerConfig, useFactory: NgxBootstrapDatePickerConfigService.getDatepickerConfig },
        { provide: BsDaterangepickerConfig, useFactory: NgxBootstrapDatePickerConfigService.getDaterangepickerConfig },
        { provide: BsLocaleService, useFactory: NgxBootstrapDatePickerConfigService.getDatepickerLocale }
    ]
})
export class MainModule { }

import { AbpModule } from '@abp/abp.module';
import * as ngCommon from '@angular/common';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppCommonModule } from '@app/shared/common/app-common.module';
import { RfidTableComponent } from './rfid-table/rfid-table.component';
import { CustomerRoutingModule } from './customer-routing.module';
import { ModalModule } from 'ngx-bootstrap';


@NgModule({
    imports: [
        ngCommon.CommonModule,
        CommonModule,
        FormsModule,
        ModalModule,
        AppCommonModule,
        CustomerRoutingModule,
        AbpModule,
    ],
    declarations: [
        RfidTableComponent
    ],
    providers: [
    ]
})
export class CustomerModule { }

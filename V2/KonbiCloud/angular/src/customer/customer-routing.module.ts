import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { RfidTableComponent } from './rfid-table/rfid-table.component'


@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: '',
                children: [
                    { path: 'rfid-table', component: RfidTableComponent },
                ]
            }
        ])
    ],
    exports: [
        RouterModule
    ]
})
export class CustomerRoutingModule { }

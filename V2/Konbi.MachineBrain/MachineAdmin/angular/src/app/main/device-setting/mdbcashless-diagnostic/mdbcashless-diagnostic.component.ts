import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { Component, OnInit, Injector } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { PaymentServiceProxy } from '@shared/service-proxies/payment-service-proxies';
import { CommonServiceProxy, GetComPortDtoList } from '@shared/service-proxies/common-service-proxies';
import { strict } from 'assert';

@Component({
    selector: 'app-mdbcashless-diagnostic',
    templateUrl: './mdbcashless-diagnostic.component.html',
    styleUrls: ['./mdbcashless-diagnostic.component.css']
})
export class MdbcashlessDiagnosticComponent extends AppComponentBase {

    public logDataContent: string;
    public comPorts: GetComPortDtoList = new GetComPortDtoList();
    constructor(
        private _paymentService: PaymentServiceProxy,
        private _commonService: CommonServiceProxy,
        injector: Injector
    ) {
        super(injector);
    }
    
    ngOnInit() {
        this.reloadLog();
        this.getComPorts();
    }

    enableSale001(): void {
        console.log("Enable sale 001");

    }

    enableSale(): void {
        console.log("Enable sale");
        this._paymentService.enablePayment().subscribe();
    }

    reloadLog(): void {
        this._commonService.getMdbLog().subscribe((str) => {
            this.logDataContent = str;
        })
    }

    getComPorts(): void {
        this._commonService.getComPorts().subscribe((st) => {
            console.log(st);
            this.comPorts = st;
        });
    }

    disableSale(): void {
        this._paymentService.disablePayment().subscribe();
    }
    reset(): void {

    }

}

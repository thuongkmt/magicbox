import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { TransactionServiceProxy, TransactionDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';

@Component({
    selector: 'magixboxTransactionDetailModal',
    templateUrl: './mt-detail.component.html'
})
export class MtDetailComponent extends AppComponentBase {

    @ViewChild('magixboxTransactionDetailModal') modal: ModalDirective;
    @ViewChild('imageModal') imageModal: ModalDirective;

    transaction: TransactionDto;
    active = false;
    lazyLoadEvent : LazyLoadEvent;
    currencySymbol = '$';
    imgUrl = '';
    defaultImage: string = 'assets/common/images/ic_nophoto.jpg'

    constructor(
        injector: Injector,
        private _transactionServiceProxy: TransactionServiceProxy
    ) {
        super(injector);
    }

    ngOnInit() {
        this.currencySymbol = abp.setting.get("CurrencySymbol");
      }

    show(record: TransactionDto): void {
        this.transaction = record;
        this.primengTableHelper.records = record.transactionDetails;
        this.active = true;
        this.modal.show();
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }

    showFullImage(url: string) {
        if (url == this.defaultImage) return;
        this.imgUrl = url;
        this.imageModal.show();
    }
    closeImageModal()
    {
        this.imageModal.hide();
        this.imgUrl = '';
    }
}

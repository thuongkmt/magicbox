import { Component, ViewChild, Injector, Output, EventEmitter, ViewEncapsulation} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import {DiscsServiceProxy, PlateLookupTableDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';

@Component({
    selector: 'plateLookupTableModal',
    styleUrls: ['./plate-lookup-table-modal.component.less'],
    encapsulation: ViewEncapsulation.None,
    templateUrl: './plate-lookup-table-modal.component.html'
})
export class PlateLookupTableModalComponent extends AppComponentBase{

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;

    filterText = '';
    id :string;
    displayName :string;
    code : string;

    

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;
    

    constructor(
        injector: Injector,
        private _discsServiceProxy: DiscsServiceProxy
    ) {
        super(injector);
        this.primengTableHelper.defaultRecordsCountPerPage = 15
    }

    show(): void {
        this.active = true;
        //this.paginator.rows = 5;
        this.getAll();
        this.modal.show();
    }

    getAll(event?: LazyLoadEvent) {
        if (!this.active) {
            return;
        }

        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();

		this._discsServiceProxy.getAllPlateForLookupTable(
			this.filterText,
            this.primengTableHelper.getSorting(this.dataTable),
            this.primengTableHelper.getSkipCount(this.paginator, event),
            this.primengTableHelper.getMaxResultCount(this.paginator, event)
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    reloadPage(): void {
        this.paginator.changePage(this.paginator.getPage());
    }

    setAndSave(plate: PlateLookupTableDto) {
        this.id = plate.id;
        this.displayName = plate.displayName;
        this.code = plate.code;

        this.active = false;
        this.modal.hide();
        this.modalSave.emit(null);
    }

    close(): void {
        this.active = false;
        this.modal.hide();
        this.modalSave.emit(null);
    }
}

import { Component, ViewChild, Injector, Output, EventEmitter, ViewEncapsulation} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
// import {PlatesServiceProxy, PlateCategoryLookupTableDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { PlateCategoryServiceProxy, PlateCategoryDto } from '@shared/service-proxies/plate-category-service-proxies';

@Component({
    selector: 'plateCategoryLookupTableModal',
    styleUrls: ['./plateCategory-lookup-table-modal.component.less'],
    encapsulation: ViewEncapsulation.None,
    templateUrl: './plateCategory-lookup-table-modal.component.html'
})
export class PlateCategoryLookupTableModalComponent extends AppComponentBase{

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;

    filterText = '';
    id :number;
    displayName :string;
    

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;
    

    constructor(
        injector: Injector,
        private _plateCategoriesServiceProxy: PlateCategoryServiceProxy
    ) {
        super(injector);
    }

    show(): void {
        this.active = true;
        this.paginator.rows = 5;
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

		this._plateCategoriesServiceProxy.getAll(
            this.filterText,
            "",
            this.primengTableHelper.getSorting(this.dataTable),
            this.primengTableHelper.getMaxResultCount(this.paginator, event),
            this.primengTableHelper.getSkipCount(this.paginator, event)
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    reloadPage(): void {
        this.paginator.changePage(this.paginator.getPage());
    }

    setAndSave(plateCategory: PlateCategoryDto) {
        this.id = plateCategory.id;
        this.displayName = plateCategory.name;

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

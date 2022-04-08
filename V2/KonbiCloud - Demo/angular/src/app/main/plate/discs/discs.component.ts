import { Component, Injector, ViewEncapsulation, ViewChild, NgZone, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Http } from '@angular/http';
import { DiscsServiceProxy, DiscDto } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { CreateOrEditDiscModalComponent } from './create-or-edit-disc-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';
import { SignalRHelper } from 'shared/helpers/SignalRHelper';

import { MessageSignalrService } from '../../../shared/common/signalr/message-signalr.service';
import { PlateServiceProxy, CreateOrEditPlateDto } from '@shared/service-proxies/plate-service-proxies';

@Component({
    templateUrl: './discs.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class DiscsComponent extends AppComponentBase implements AfterViewInit {

    @ViewChild('createOrEditDiscModal') createOrEditDiscModal: CreateOrEditDiscModalComponent;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;

    discsHeaderInfo = '';

    advancedFiltersAreShown = false;
    filterText = '';
    uidFilter = '';
    codeFilter = '';

    plateNameFilter = '';
    plateCodeFilter = '';
    plateIdFilter = '';

    constructor(
        injector: Injector,
        private _discsServiceProxy: DiscsServiceProxy,
        private _platesServiceProxy: PlateServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _activatedRoute: ActivatedRoute,
        private _fileDownloadService: FileDownloadService,
        private route: ActivatedRoute,
        private _messageSignalrService: MessageSignalrService,
        public _zone: NgZone
    ) {
        super(injector);

        this.route.params.forEach((params: Params) => {
            if (params['plate_id'] !== undefined) {
                this.plateIdFilter = params['plate_id']
                this.getPlateInfo(params['plate_id'])
            }
        });
    }

    ngOnInit(): void {}

    ngAfterViewInit() {
        // if (this.plateIdFilter == '') {
        //     this.createOrEditDiscModal.show(this.plateIdFilter, this.plateCodeFilter, this.plateNameFilter);
        // }
    }

    getPlateInfo(plateId): void {
        this._platesServiceProxy.get(plateId).subscribe(result => {
            this.plateIdFilter = result.plate.id
            this.plateNameFilter = result.plate.name
            this.plateCodeFilter = result.plate.code
            this.discsHeaderInfo = "Plate Code " + result.plate.code

            this.createOrEditDiscModal.show(this.plateIdFilter, this.plateCodeFilter, this.plateNameFilter);
            //this.dataTable.onLazyLoad.emit();
            this.getDiscs()
        });
    }

    getDiscs(event?: LazyLoadEvent) {
        // if (this.primengTableHelper.shouldResetPaging(event)) {
        //     this.paginator.changePage(0);
        //     return;
        // }
        this._discsServiceProxy.getAll(
            this.filterText,
            this.uidFilter,
            this.codeFilter,
            this.plateNameFilter,
            this.plateIdFilter,
            this.primengTableHelper.getSorting(this.dataTable),
            0,
            1000
            // this.primengTableHelper.getSkipCount(this.paginator, event),
            // this.primengTableHelper.getMaxResultCount(this.paginator, event)
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
        });
    }

    reloadPage(): void {
        this.paginator.changePage(this.paginator.getPage());
    }

    createDisc(): void {
        this.createOrEditDiscModal.show(this.plateIdFilter, this.plateCodeFilter, this.plateNameFilter);
    }

    deleteDisc(disc: DiscDto): void {
        this.message.confirm(
            '',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._discsServiceProxy.delete(disc.id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }

    exportToExcel(): void {
        this._discsServiceProxy.getDiscsToExcel(
            this.filterText,
            this.uidFilter,
            this.codeFilter,
            this.plateNameFilter,
        )
            .subscribe(result => {
                this._fileDownloadService.downloadTempFile(result);
            });
    }


}
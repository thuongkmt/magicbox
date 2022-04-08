import { Component, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Http } from '@angular/http';
import { PlateServiceProxy, PlateDto, CreateOrEditPlateDto, ImportResult } from '@shared/service-proxies/plate-service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { CreateOrEditPlateModalComponent } from './create-or-edit-plate-modal.component';
import { ImportPlateModalComponent } from './import-plate-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import { Papa } from 'ngx-papaparse';
import { truncate } from 'fs';
import { PlateCategoryServiceProxy, PlateCategoryDto } from '@shared/service-proxies/plate-category-service-proxies';

@Component({
    templateUrl: './plates.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class PlatesComponent extends AppComponentBase {

    @ViewChild('createOrEditPlateModal') createOrEditPlateModal: CreateOrEditPlateModalComponent;
    @ViewChild('importPlateModal') importPlateModal: ImportPlateModalComponent;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;
    @ViewChild('import') myInputVariable: ElementRef;

    advancedFiltersAreShown = true;
    filterText = '';
    nameFilter = '';
    imageUrlFilter = '';
    descFilter = '';
    codeFilter = '';
    maxAvaiableFilter: number;
    maxAvaiableFilterEmpty: number;
    minAvaiableFilter: number;
    minAvaiableFilterEmpty: number;
    colorFilter = '';
    plateCategoryNameFilter = '';
    categories: PlateCategoryDto[] = [];

    constructor(
        injector: Injector,
        private _platesServiceProxy: PlateServiceProxy,
        private _notifyService: NotifyService,
        private _tokenAuth: TokenAuthServiceProxy,
        private _activatedRoute: ActivatedRoute,
        private _fileDownloadService: FileDownloadService,
        private _plateCategoriesServiceProxy: PlateCategoryServiceProxy,
        private papa: Papa
    ) {
        super(injector);
        this.primengTableHelper.defaultRecordsCountPerPage = 25
    }

    ngOnInit() {
        this.getPlateCategories();
      }

    getPlateCategories() {
        this._plateCategoriesServiceProxy.getAll(
            '',
            '',
            '',
            100,
            0
        ).subscribe(result => {
            result.items.forEach(element => {
                this.categories.push(element.plateCategory);
            });
        });
    }

    getPlates(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();
        this._platesServiceProxy.getAll(
            this.filterText,
            this.nameFilter,
            this.codeFilter,
            this.colorFilter,
            this.plateCategoryNameFilter,
            this.primengTableHelper.getSorting(this.dataTable),
            this.primengTableHelper.getMaxResultCount(this.paginator, event),
            this.primengTableHelper.getSkipCount(this.paginator, event)
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
            console.log(this.primengTableHelper.records);
            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    reloadPage(): void {
        this.paginator.changePage(this.paginator.getPage());
    }

    createPlate(): void {
        this.createOrEditPlateModal.show();
    }

    deletePlate(plate: PlateDto): void {
        this.message.confirm(
            'Are you sure you want to delete plate?',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._platesServiceProxy.delete(plate.id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }

    exportToCsv(): void {
        if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
            this.primengTableHelper.showLoadingIndicator();
            this._platesServiceProxy.getAll(
                this.filterText,
                this.nameFilter,
                this.codeFilter,
                this.colorFilter,
                this.plateCategoryNameFilter,
                this.primengTableHelper.getSorting(this.dataTable),
                1000,
                0
            ).subscribe(result => {
                if (result.items != null) {
                    let csvData = new Array();
                    const header = {
                        PlateCategoryId: 'Plate Category Id',
                        PlateCategoryName: 'Plate Category Name',
                        PlateName: 'Plate Name',
                        PlateImage: 'Plate Image',
                        PlateDescription: 'Plate Description',
                        PlateCode: 'Plate Code',
                        PlateColor: 'Plate Color'
                    }
                    csvData.push(header)
                    for (let record of result.items) {
                        csvData.push({
                            PlateCategoryId: (record.plate.plateCategoryId == null) ? '' : record.plate.plateCategoryId,
                            PlateCategoryName: (record.plateCategoryName == null) ? '' : record.plateCategoryName,
                            PlateName: (record.plate.name == null) ? '' : record.plate.name,
                            PlateImage: (record.plate.imageUrl == null) ? '' : record.plate.imageUrl,
                            PlateDescription: (record.plate.desc == null) ? '' : record.plate.desc,
                            PlateCode: record.plate.code,
                            PlateColor: (record.plate.color == null) ? '' : record.plate.color
                        })
                    }
                    new Angular5Csv(csvData, 'Plates');
                }
                else {
                    this.notify.info(this.l('DateEmpty'));
                }
                this.primengTableHelper.hideLoadingIndicator();
            });
        }
        else {
            this.notify.info(this.l('DateEmpty'));
        }
    }

    importCsvClick(){
        this.importPlateModal.show();
    }

    importCSV(files) {
        if (files.length === 0)
            return;
        var file: File = files[0];
        //validate file type
        let valToLower = file.name.toLowerCase();
        let regex = new RegExp("(.*?)\.(csv)$");
        if (!regex.test(valToLower)) {
            abp.message.error('Please select csv file');
            this.myInputVariable.nativeElement.value = '';
            return
        }

        let myReader = new FileReader();
        myReader.onloadend = (e) => {
            let fileContent = myReader.result.toString();
            this.papa.parse(fileContent, {
                header: true,
                delimiter: ",",
                worker: false,
                skipEmptyLines: true,
                complete: (result) => {
                    var errorList = '';
                    if (result.errors.length > 0) {

                        result.errors.forEach(element => {
                            errorList += ('<div>' + element + '</div>')
                        });
                        //var errorList = result.errors.map(e => e.row).join(",");
                        var mess = new ImportResult();
                        mess.successCount = 0;
                        mess.errorCount = result.errors.length;
                        mess.errorList = errorList;
                        this.showImportMessage(mess, "Invalid Data");
                        this.myInputVariable.nativeElement.value = "";
                        return;
                    }

                    //check empty plate code
                    let csvImportData = new Array<CreateOrEditPlateDto>();
                    let errorCount = 0;
                    for (var i = 0; i < result.data.length; i++) {
                        let record = result.data[i]
                        let item = new CreateOrEditPlateDto();
                        item.plateCategoryId = parseInt(record["Plate Category Id"])
                        item.name = record["Plate Name"]
                        item.desc = record["Plate Description"]
                        item.code = record["Plate Code"]
                        item.color = record["Plate Color"]
                        item.imageUrl = record["Plate Image"]

                        if (item.code == '') {
                            errorList += ('<div>Row ' + (i + 2) + ' late code is empty</div>')
                            errorCount ++
                        }
                        csvImportData.push(item)
                    }

                    if (errorList != '') {
                        var mess = new ImportResult();
                        mess.successCount = 0;
                        mess.errorCount = errorCount;
                        mess.errorList = errorList;
                        this.showImportMessage(mess, "Invalid Data");
                        this.myInputVariable.nativeElement.value = "";
                        return;
                    }

                    //check duplicate plate code
                    var sorted_arr = csvImportData.slice().sort((a, b) => b.code < a.code ? 1 : -1);;
                    var resultsError = [];
                    for (var i = 0; i < sorted_arr.length - 1; i++) {
                        if (sorted_arr[i + 1].code == sorted_arr[i].code) {
                            resultsError.push(sorted_arr[i]);
                        }
                    }
                    if (resultsError.length > 0) {
                        resultsError.forEach(element => {
                            errorList += ('<div>Duplicate plate code ' + element.code + '</div>')
                        });
                        var mess = new ImportResult();
                        mess.successCount = 0;
                        mess.errorCount = resultsError.length;
                        mess.errorList = errorList;
                        this.showImportMessage(mess, "Invalid Data");
                        this.myInputVariable.nativeElement.value = "";
                        return;
                    }
                    // validate ok, submit data
                    this._platesServiceProxy.importPlate(csvImportData)
                        .subscribe((result) => {
                            this.showImportMessage(result, "Import Result");
                            this.myInputVariable.nativeElement.value = "";
                            this.reloadPage();
                        });
                }
            });
        }
        myReader.readAsText(file);
    }

    showImportMessage(result: ImportResult, title: string) {
        var content = "<div class='text-left' style='margin-left: 20px;'><div>- Success: " + result.successCount + "</div>";
        content += "<div>- Failed: " + result.errorCount + "</div>";
        content += "<div>- Failed Rows: " + result.errorList + "</div></div>";

        abp.message.info(content, title, true);
    }

}
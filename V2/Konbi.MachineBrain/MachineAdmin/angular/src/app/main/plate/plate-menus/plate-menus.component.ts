import { Component, Injector, ViewEncapsulation, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Http } from '@angular/http';
import { PlateServiceProxy, PlateDto } from '@shared/service-proxies/plate-service-proxies';
//import { SessionsServiceProxy, SessionDto } from '@shared/service-proxies/service-proxies';
import { PlateMenuServiceProxy, PlateMenuDto, PlateMenuInputDto, ImportResult, ImportData } from '@shared/service-proxies/plate-menu-service-proxies';
import { PlateCategoryServiceProxy, PlateCategoryDto } from '@shared/service-proxies/plate-category-service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import * as _ from 'lodash';
import * as moment from 'moment';
import { Papa } from 'ngx-papaparse';
import { ReplicateComponent } from './replicate/replicate.component';

@Component({
    templateUrl: './plate-menus.component.html',
    styleUrls: ['./plate-menus.component.css'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class PlateMenusComponent extends AppComponentBase {

    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;
    @ViewChild('import') myInputVariable: ElementRef;
    @ViewChild('replicateModal') replicateModal: ReplicateComponent;

   // sessions: SessionDto[] = [];
    categories: PlateCategoryDto[] = [];

    dateFilter;
    sessionId = null;
    paramSessionId = null;

    categoryId = '0'

    nameFilter = '';
    codeFilter = '';
    colorFilter = '';
    firstTimeLoadData = true;
    lazyLoadEvent: LazyLoadEvent;

    constructor(
        injector: Injector,
        private _plateMenusServiceProxy: PlateMenuServiceProxy,
       // private _sessionsServiceProxy: SessionsServiceProxy,
        private _plateCategoriesServiceProxy: PlateCategoryServiceProxy,
        private papa: Papa,
        private route: ActivatedRoute,
        private changeDetectorRef: ChangeDetectorRef,
    ) {
        super(injector);
    }

    ngOnInit() {
        this.route.params.forEach((params: Params) => {
            if (params['datefilter'] !== undefined) {
                this.dateFilter = params['datefilter']
            } else {
                var current = new Date();
                var month = ("0" + current.getMonth() + 1).slice(-2);
                var date = ("0" + current.getDate()).slice(-2);
                var currentDate = current.getFullYear() + '-' + month + '-' + date;
                this.dateFilter = currentDate;
            }
            if (params['sessionfilter'] !== undefined) {
                this.paramSessionId = params['sessionfilter']
            }
        });

        this.getSessions();
        this.getPlateCategories();
    }

    getSessions() {
        // this.primengTableHelper.showLoadingIndicator();
        // this._sessionsServiceProxy.getAll(
        //     '',
        //     '',
        //     '',
        //     '',
        //     '',
        //     0,
        //     100
        // ).subscribe(result => {
        //     if (result.items != null) {
        //         var now = new Date();
        //         var hours = now.getHours().toString() + now.getMinutes();
        //         result.items.forEach(element => {
        //             this.sessions.push(element.session);
        //             if (this.paramSessionId != null) {
        //                 this.sessionId = this.paramSessionId;
        //             } else if (element.session.toHrs != null) {
        //                 if (hours < element.session.toHrs) {
        //                     if (element.session.fromHrs != null) {
        //                         if (hours >= element.session.fromHrs) {
        //                             this.sessionId = element.session.id;
        //                         }
        //                     }
        //                 }
        //             }
        //             else if (element.session.fromHrs != null) {
        //                 if (hours >= element.session.fromHrs) {
        //                     this.sessionId = element.session.id;
        //                 }
        //             }
        //         });
        //         if (this.sessionId == null && this.sessions.length > 0) {
        //             this.sessionId = this.sessions[0].id;
        //         }
        //         this.callPlateMenusApi(this.lazyLoadEvent);
        //     }
        // });
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

    getPlateMenus(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }
        if (this.firstTimeLoadData) {
            this.firstTimeLoadData = false;
            this.lazyLoadEvent = event;
            return;
        }
        this.callPlateMenusApi(event);
    }

    callPlateMenusApi(event?: LazyLoadEvent) {
        this.primengTableHelper.showLoadingIndicator();
        this._plateMenusServiceProxy.GetAllPlateMenus(
            this.dateFilter,
            this.sessionId,
            this.nameFilter,
            this.codeFilter,
            this.categoryId,
            this.colorFilter,
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
        if (this.dateFilter == null || this.dateFilter.length == 0) {
            this.primengTableHelper.records = [];
            this.notify.info(this.l('DateEmpty'));
            return;
        }
        this.paginator.changePage(this.paginator.getPage());
    }

    exportToCsv(): void {
        // if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
        //     this.primengTableHelper.showLoadingIndicator();
        //     this._plateMenusServiceProxy.GetAllPlateMenus(
        //         this.dateFilter,
        //         this.sessionId,
        //         this.nameFilter,
        //         this.codeFilter,
        //         this.categoryId,
        //         this.colorFilter,
        //         this.primengTableHelper.getSorting(this.dataTable),
        //         1000,
        //         0
        //     ).subscribe(result => {
        //         if (result.items != null) {
        //             let csvData = new Array();
        //             const header = {
        //                 Date: 'Date',
        //                 Session: 'Session',
        //                 CategoryCode: 'Category Code',
        //                 PlateCode: 'Plate Code',
        //                 Price: 'Price',
        //                 ContractorPrice: 'Contractor Price'
        //             }
        //             csvData.push(header)

        //             var sessions = this.sessions.filter(x => x.id == this.sessionId);
        //             if (sessions != null && sessions.length > 0) {
        //                 for (let record of result.items) {
        //                     csvData.push({
        //                         Date: this.dateFilter,
        //                         Session: sessions[0].name,
        //                         CategoryCode: record.categoryName,
        //                         PlateCode: record.plate.code,
        //                         Price: record.price,
        //                         ContractorPrice: record.priceStrategy
        //                     })
        //                 }
        //                 new Angular5Csv(csvData, 'Menu Scheduling');
        //             }
        //             else {
        //                 this.notify.info(this.l('DateEmpty'));
        //             }
        //         }
        //         else {
        //             this.notify.info(this.l('DateEmpty'));
        //         }
        //         this.primengTableHelper.hideLoadingIndicator();
        //     });
        // }
        // else {
        //     this.notify.info(this.l('DateEmpty'));
        // }
    }

    updatePrice(record: PlateMenuDto) {
        if (record == null || record == undefined) return;
        if (record.price < 0) {
            this.notify.info(this.l('PriceZero'));
            return;
        }
        var input = new PlateMenuInputDto();
        input.id = record.id;
        input.price = record.price;

        this._plateMenusServiceProxy.updatePrice(input).subscribe(result => {
            if (result == true) {
                this.notify.info(this.l('SavedSuccessfully'));
            }
            else {
                this.notify.info(this.l('SaveFailed'));
            }
        });
    }
    updatePriceStrategy(record: PlateMenuDto) {
        if (record == null || record == undefined) return;
        if (record.priceStrategy < 0) {
            this.notify.info(this.l('PriceZero'));
            return;
        }
        var input = new PlateMenuInputDto();
        input.id = record.id;
        input.priceStrategyId = record.priceStrategyId;
        input.priceStrategy = record.priceStrategy;

        this._plateMenusServiceProxy.updatePriceStrategy(input).subscribe(result => {
            if (result == true) {
                this.notify.info(this.l('SavedSuccessfully'));
            }
            else {
                this.notify.info(this.l('SaveFailed'));
            }
        });
    }

    importCSV(files) {
        if (files.length === 0) return;
        var file: File = files[0];
        //validate file type
        let valToLower = file.name.toLowerCase();
        let regex = new RegExp("(.*?)\.(csv)$");
        if (!regex.test(valToLower)) {
            abp.message.error('Please select csv file');
            this.myInputVariable.nativeElement.value = '';
            return;
        }

        let myReader = new FileReader();
        myReader.onloadend = (e) => {
            let fileContent = myReader.result.toString();
            if (fileContent != null && fileContent != undefined && fileContent.trim().length == 0) {
                abp.message.info("File empty");
                this.myInputVariable.nativeElement.value = "";
                return;
            }
            this.papa.parse(fileContent, {
                header: true,
                delimiter: ",",
                worker: false,
                skipEmptyLines: true,
                complete: (result) => {
                    if (result.errors.length > 0) {
                        var errorList = result.errors.map(e => e.row).join(",");
                        var mess = new ImportResult();
                        mess.successCount = 0;
                        mess.errorCount = 0;
                        mess.errorList = errorList;
                        this.showImportMessage(mess, "Invalid Data");
                        this.myInputVariable.nativeElement.value = "";
                        return;
                    }

                    let importData = new Array<ImportData>();
                    for (let record of result.data) {
                        let item = new ImportData();
                        item.selectedDate = record["Date"].trim();
                        item.sessionName = record["Session"].trim();
                        item.plateCode = record["Plate Code"].trim();
                        item.price = record["Price"].trim();
                        importData.push(item);
                    }
                    //submit data
                    this._plateMenusServiceProxy.importPlateMenu(importData)
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

    syncData() {
        this.primengTableHelper.showLoadingIndicator();
        this._plateMenusServiceProxy.syncData()
            .finally(() => {
                this.primengTableHelper.hideLoadingIndicator();
            })
            .subscribe(() => {
                this.reloadPage();
                this.primengTableHelper.hideLoadingIndicator();
                this.notify.success('Data synced');
            });
    }
    replicateData() {
        this.replicateModal.show(this.dateFilter)
    }
}

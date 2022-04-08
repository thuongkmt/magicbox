import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { PlateDto } from '@shared/service-proxies/plate-service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { PlateServiceProxy, ImportResult, CreateOrEditPlateDto } from '@shared/service-proxies/plate-service-proxies';
import { Papa } from 'ngx-papaparse';

@Component({
    selector: 'importPlateModal',
    templateUrl: './import-plate-modal.component.html',
    styleUrls: ['./create-or-edit-plate-modal.component.css']
})
export class ImportPlateModalComponent extends AppComponentBase {

    @ViewChild('importPlateModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    @ViewChild('import') myInputVariable: ElementRef;

    importStep = 1;

    active = false;
    saving = false;

    //upload photo
    files: File[] = []
    lastInvalids: any;
    lastFileAt: Date

    getDate() {
        // if (this.files.length > 3) {
        //     alert("Please select maximun 3 images");
        //     this.files = []
        // }
        return new Date()
    }

    constructor(
        injector: Injector,
        private _platesServiceProxy: PlateServiceProxy,
        private papa: Papa
    ) {
        super(injector);
    }

    show(): void {
        this.active = true;
        this.modal.show();
    }

    close(): void {
        this.importStep = 1;
        this.files = []
        if (this.myInputVariable) this.myInputVariable.nativeElement.value = '';
        this.active = false;
        this.saving = false;
        this.modal.hide();
    }

    btnNextToStep2Click() {

        //upload images first
        if (this.files.length > 0) {
            this.saving = true;
            this._platesServiceProxy.importCsvUploadImages(this.files)
                .subscribe(
                    response => {
                        this.saving = false;
                        if (response.result) {
                            this.importStep = 2;
                        } else {
                            abp.message.error("Upload error, please check image files and try again");
                        }
                    }, err => {
                        console.log(err)
                        abp.message.error("Upload error, please check image files and try again");
                    }
                )
        } else {
            this.importStep = 2;
        }
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
                            errorCount++;
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
                            this.close();
                            this.modalSave.emit(null);
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

import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { AppComponentBase } from '@shared/common/app-component-base';
import { HttpClient } from '@angular/common/http';
import { AppConsts } from '@shared/AppConsts';
import { ProductsServiceProxy,CreateOrEditProductDto } from '@shared/service-proxies/service-proxies';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'importExcelProductModal',
  templateUrl: './import-product-modal.component.html'
})
export class ImportProductModalComponent extends AppComponentBase {
  private httpClient: HttpClient;
  private url: string;
  public importProducts: CreateOrEditProductDto[];

  constructor(
    injector: Injector,
    private http: HttpClient,
    private _productsServiceProxy: ProductsServiceProxy
  ) {
    super(injector);
    this.httpClient = http;
    this.url = AppConsts.remoteServiceBaseUrl;
  }

  @ViewChild('importProductModal') modal: ModalDirective;
  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

  active = false;
  saving = false;

  show() {
    this.active = true;
    this.modal.show();
  }

  close(): void {
    this.active = false;
    this.modal.hide();
  }

  import() {
    this.close();
    this._productsServiceProxy.import(this.importProducts)
        .pipe(finalize(() => { this.saving = false; }))
        .subscribe(() => {
            this.notify.info(this.l('SavedSuccessfully'));
            this.close();
            this.modalSave.emit(null);
        });
  }


  isCSVFile(file: any) {
    return file.name.endsWith('.csv');
  }
  getHeaderArray(csvRecordsArr: any) {
    let headers = csvRecordsArr[0].split(',');
    let headerArray = [];
    for (let j = 0; j < headers.length; j++) {
      headerArray.push(headers[j]);
    }
    return headerArray;
  }
  
  getDataRecordsArrayFromCSVFile(csvRecordsArray: any, headerLength: any) {
    this.importProducts = [];
    for (let i = 1; i < csvRecordsArray.length; i++) {
      let data = csvRecordsArray[i].split(',');
      // FOR EACH ROW IN CSV FILE IF THE NUMBER OF COLUMNS
      // ARE SAME AS NUMBER OF HEADER COLUMNS THEN PARSE THE DATA
      if (data.length === headerLength) {

        let csvRecord: CreateOrEditProductDto = new CreateOrEditProductDto();

        csvRecord.name = data[0].trim();
        csvRecord.sku = data[1].trim();
        csvRecord.price = data[2].trim();
        csvRecord.barcode = data[3].trim();
        csvRecord.categoryNames = data[4].trim();

        this.importProducts.push(csvRecord);
      }
    }
  }
  fileChangeListener($event: any): void {
    let text = [];
    let files = $event.srcElement.files;

    if (this.isCSVFile(files[0])) {

      let input = $event.target;
      let reader = new FileReader();
      reader.readAsText(input.files[0]);

      reader.onload = (data) => {
        let csvData = reader.result;
        let csvRecordsArray = (csvData as string).split(/\r\n|\n/);
        let headersRow = this.getHeaderArray(csvRecordsArray);
        this.getDataRecordsArrayFromCSVFile(csvRecordsArray, headersRow.length);
      };

      reader.onerror = function () {
        alert('Unable to read ' + input.files[0]);
      };

    } else {
      alert('Please import valid .csv file.');
    }
  }

}


import { Component, OnInit } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Inject } from "@angular/core";
import { NgbModal, ModalDismissReasons } from '@ng-bootstrap/ng-bootstrap';


@Component({
  selector: 'app-product',
  templateUrl: './product.component.html',
  styleUrls: ['./product.component.css']
})
export class ProductComponent implements OnInit {
  public products: Product[];
  public importProducts: Product[];

  public closeResult: string;
  public selectedProduct: Product;
  public modalHeader: string;
  public modalButtonText: string;
  private url: string;
  private httpClient: HttpClient;


  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private modalService: NgbModal) {
    this.selectedProduct = new Product();
    this.selectedProduct.productName = "";
    this.selectedProduct.sku = "";
    this.selectedProduct.price = 0;
    this.httpClient = http;
    this.url = baseUrl;
    this.reload();
  }
  import() {
    this.httpClient.post<Product>(this.url + 'api/Product/Import', this.importProducts).subscribe(result => {
      //alert("Add successful");
      this.reload();
    }, error => {
      alert("Import product failed!");
      console.error(error);
    });
  }

  insert() {
    this.httpClient.post<Product>(this.url + 'api/Product/Insert', this.selectedProduct).subscribe(result => {
      //alert("Add successful");
      this.reload();
    }, error => {
      alert("Add product failed!");
      console.error(error);
    });
  }
  update() {
    this.httpClient.post<Product>(this.url + 'api/Product/Update', this.selectedProduct).subscribe(result => {
      //alert("Add successful");
      this.reload();
    }, error => {
      alert("Update product failed!");
      console.error(error);
    });
  }
  get(id) {
    this.httpClient.get<Product>(this.url + 'api/Product/Get?id=' + id).subscribe(result => {
      this.selectedProduct = result;
      console.log("> " + this.selectedProduct);
    }, error => console.error(error));
  }
  edit(id, content) {
    this.open(content, 'Edit');
    this.get(id);
  }
  delete(id) {
    var ok = confirm("Delete this product?");
    if (ok) {
      this.httpClient.post<Product>(this.url + 'api/Product/Delete', id).subscribe(result => {
        this.reload();
      }, error => {
        alert("Delete product failed!");
        console.error(error);
      });
    }
  }
  reload() {
    this.httpClient.get<Product[]>(this.url + 'api/Product/GetAll').subscribe(result => {
      this.products = result;
      console.log(this.products);
    }, error => console.error(error));
  }

  open(content, type) {
    this.selectedProduct = new Product();
    if (type == 'Import') {
      this.modalHeader = "Import Product";
      this.modalButtonText = "Import";
    }
    if (type == 'Add') {
      this.modalHeader = "Add Product";
      this.modalButtonText = "Add";
    }
    if (type == 'Edit') {
      this.modalHeader = "Edit Product";
      this.modalButtonText = "Save";
    }
    // Close button
    this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' }).result.then((result) => {
      //this.closeResult = `Closed with: ${content.toString()}` + type;
      if (type == 'Import') {
        this.import();
        this.importProducts = [];
      }
      if (type == 'Add') {
        this.insert();
      }
      if (type == 'Edit') {
        this.update();
      }
    }, (reason) => {
      //this.closeResult = `Dismissed ${this.getDismissReason(reason)}`;
    });
  }

  isCSVFile(file: any) {
    return file.name.endsWith(".csv");
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

    this.importProducts =[];
    for (let i = 1; i < csvRecordsArray.length; i++) {
      let data = csvRecordsArray[i].split(',');
      // FOR EACH ROW IN CSV FILE IF THE NUMBER OF COLUMNS
      // ARE SAME AS NUMBER OF HEADER COLUMNS THEN PARSE THE DATA
      if (data.length == headerLength) {

        var csvRecord: Product = new Product();

        csvRecord.productName = data[0].trim();
        csvRecord.sku = data[1].trim();
        csvRecord.price = data[2].trim();

        this.importProducts.push(csvRecord);
      }
    }
 
  }
  fileChangeListener($event: any): void {

    var text = [];
    var files = $event.srcElement.files;

    if (this.isCSVFile(files[0])) {

      var input = $event.target;
      var reader = new FileReader();
      reader.readAsText(input.files[0]);

      reader.onload = (data) => {
        let csvData = reader.result;
        let csvRecordsArray = csvData.split(/\r\n|\n/);
        let headersRow = this.getHeaderArray(csvRecordsArray);
        this.getDataRecordsArrayFromCSVFile(csvRecordsArray, headersRow.length);
        console.log(this.products);
      }

      reader.onerror = function () {
        alert('Unable to read ' + input.files[0]);
      };



    } else {
      alert("Please import valid .csv file.");
    //  t//his.fileReset();
    }
  }
  ngOnInit() {
  }
}
class Product {
  id: number;
  productName: string;
  sku: string;
  price: number;
  createdData: string;
  createdBy: string;
  updatedDate: string;
  updatedBy: string;
  isDeleted: boolean;
}

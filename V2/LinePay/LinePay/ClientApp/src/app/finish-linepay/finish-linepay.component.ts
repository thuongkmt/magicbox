import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NgxSpinnerService } from 'ngx-spinner';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-finish-linepay',
  templateUrl: './finish-linepay.component.html',
  styleUrls: ['./finish-linepay.component.css']
})
export class FinishLinepayComponent implements OnInit {

  public transactionId = '';
  public linePay: LinePayFinishDto;
  public arrayProduct = [];

  constructor(private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private route: ActivatedRoute,
    private spinner: NgxSpinnerService) {
    this.route.queryParams.subscribe(params => {
      this.transactionId = params['transactionId'];
    });
  }

  ngOnInit() {
    /** spinner starts on init */
    this.spinner.show();

    this.http.get<LinePayFinishDto>(this.baseUrl + 'api/LinePay/success?transactionId=' + this.transactionId)
      .pipe(finalize(() => {
        /** spinner ends on init */
        this.spinner.hide();
      }))
      .subscribe(result => {
        this.linePay = result;
        this.arrayProduct = this.linePay.productName.split(',');

        setTimeout(() => {
          document.location.href = 'http://localhost:5000/';
        }, 10000);
      }, error => {

      });
  }

}

export class LinePayFinishDto {
  regkey: boolean;
  transactionId: string;
  amount: number;
  productName: string;
}

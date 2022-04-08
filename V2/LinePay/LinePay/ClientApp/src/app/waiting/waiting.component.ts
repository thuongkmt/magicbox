import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { finalize } from 'rxjs/operators';
import { NgxSpinnerService } from "ngx-spinner";
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-waiting',
  templateUrl: './waiting.component.html',
  styleUrls: ['./waiting.component.css']
})
export class WaitingComponent implements OnInit {

  public transactionId = '';

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

    this.http.get<Waiting>(this.baseUrl + 'api/LinePay/waiting?transactionId=' + this.transactionId)
      .pipe(finalize(() => {

      }))
      .subscribe(result => {
        document.location.href = 'http://localhost:5000/finish-linepay?transactionId=' + this.transactionId;
      }, error => {

      });
  }

}

export class Waiting {
  capture: boolean;
}

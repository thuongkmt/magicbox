import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { finalize } from 'rxjs/operators';
import { NgxSpinnerService } from "ngx-spinner";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {

  constructor(private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private spinner: NgxSpinnerService) { }

  clickLinePay() {
    this.http.get(this.baseUrl + 'api/LinePay/reserve')
      .pipe(finalize(() => {

      }))
      .subscribe(result => {
        
      }, error => {
        
      });
  }
}

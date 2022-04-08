import { Component, OnInit } from '@angular/core';
import { UserHistory,CreditService } from '../services/credit.service';

@Component({
  selector: 'app-credit-history',
  templateUrl: './credit-history.component.html',
  styleUrls: ['./credit-history.component.css']
})
export class CreditHistoryComponent implements OnInit {

  userHistories: UserHistory[];

  constructor(private creditService:CreditService) { }

  ngOnInit() {
    this.reloadCreditHistories();
  }

  reloadCreditHistories(){
    this.creditService.getCreditHistories('hadoan').subscribe((data) => {
      console.log(data.result);
      this.userHistories = data.result;
    });
  }
}


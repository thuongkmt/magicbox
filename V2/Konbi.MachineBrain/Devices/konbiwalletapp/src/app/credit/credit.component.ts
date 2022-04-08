import { Component, OnInit } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material';

import { RxStompService } from '@stomp/ng2-stompjs';
import { Message } from '@stomp/stompjs';
import { Subscription } from 'rxjs';
import { CreditService } from '../services/credit.service';
import { AppConsts } from '../AppConsts';
import { strictEqual } from 'assert';


@Component({
  selector: 'app-credit',
  templateUrl: './credit.component.html',
  styleUrls: ['./credit.component.css']
})
export class CreditComponent implements OnInit {
  private topicSubscription: Subscription;
  public currentCredit: number;
  private topups:string[];
  private topupDialogRef:MatDialogRef<TopupStartDialogComponent>;

  constructor(public dialog: MatDialog,
    private rxStompService: RxStompService,
    private creditService: CreditService) {
     this.topups=[]; 
  }

  ngOnInit() {

    this.topicSubscription = this.rxStompService.watch('/topic/demo').subscribe((message: Message) => {
      console.log(message.body);
      if (message.body === "HADOAN_CONFIRM_OPEN") {
        this.openConfirmDialog();
      }
      else if(message.body === "TRANSACTION_COMPLETED")
      {
        this.reloadUserCredit();
        const dialogRef = this.dialog.open(TransactionSuccessDialogComponent);
        dialogRef.afterClosed().subscribe();
      }
      else if(message.body === "HADOAN_TOPUP_COMPLETED")//topup completed
      {
        this.reloadUserCredit();
      }
      else if(message.body.startsWith("HADOAN_TOPUP"))//USERNAME_TOPUP_VALUE (CENTS)
      {
        let centsStr=message.body.substring("HADOAN_TOPUP_".length);
        let cents=parseInt(centsStr);
        console.log(cents+ ' added');
        var dollar=cents/100;
        this.topups.push("Added "+dollar);
        this.topupDialogRef.componentInstance.topups=this.topups;
        this.reloadUserCredit();
      }
    });

    this.reloadUserCredit();
  }

  reloadUserCredit(){
    this.creditService.getUserCredit('hadoan').subscribe((data) => {
      this.currentCredit = data.result;
    });
  }

  ngOnDestroy() {
    this.topicSubscription.unsubscribe();
  }

  openButtonClick() {
    this.openConfirmDialog();
    // const dialogRef = this.dialog.open(OpenDoorConfirmDialogComponent);
    // dialogRef.afterClosed().subscribe();
    // this.creditService.getUserCredit('hadoan').subscribe((data) => {
    //   console.log(data)
    //   console.log(data.result);
    // });
  }


  openConfirmDialog() {
    const dialogRef = this.dialog.open(SelectActionDialogComponent);

    dialogRef.afterClosed().subscribe(result => {
      console.log(`Dialog result: ${result}`);
      if (result==2) {
        //open fridge
        this.creditService.startTransaction().subscribe(()=>{
          this.dialog.open(TransactionStartDialogComponent).afterClosed().subscribe(()=>{});
        });
      }
      else if(result==1)//topup
      {
        console.log('select topup');
        this.creditService.startTopup().subscribe();
        this.topupDialogRef=this.dialog.open(TopupStartDialogComponent);
        this.topupDialogRef.afterClosed().subscribe(()=>{});
      }
    });
  }
}


@Component({
  selector: 'select-action-dialog',
  templateUrl: 'select-action-dialog.html',
})
export class SelectActionDialogComponent { }



@Component({
  selector: 'transaction-success-dialog',
  templateUrl: 'transaction-success-dialog.html',
})
export class TransactionSuccessDialogComponent { }


@Component({
  selector: 'transaction-start-dialog',
  templateUrl: 'transaction-start-dialog.html',
})
export class TransactionStartDialogComponent { }

@Component({
  selector: 'topup-start-dialog',
  templateUrl: 'topup-start-dialog.html',
})
export class TopupStartDialogComponent { 
  public topups:string[];
  /**
   *
   */
  constructor() {
    this.topups=[]
  }
}

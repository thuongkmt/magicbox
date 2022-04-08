import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConsts } from '../AppConsts';


@Injectable({
  providedIn: 'root'
})
export class CreditService {


  constructor(private http: HttpClient) { 
  }

  getUserCredit(username): Observable<any>
  {
    
    let url=AppConsts.webAdminBackendUrl;
    console.log('getusercredit '+username+' at '+url);
      var data = this.http.get<any>(url+'/api/services/app/UserCredits/GetUserCredit?userName='+username);
     return data;
  }

  startTransaction():Observable<any>
  {
    let url=AppConsts.magicBoxBackendurl;
    var post = this.http.post(url+"/api/machine/starttransaction",null);
    return post;
  }

  getCreditHistories(userName):Observable<any>{
    let url=AppConsts.webAdminBackendUrl;
    console.log('getCreditHistories '+userName+' at '+url);
      var data = this.http.get<any>(url+'/api/services/app/UserCredits/GetUserCreditHistory?userName='+userName);
     return data;
  }

  startTopup():Observable<any>{
    let url=AppConsts.webAdminBackendUrl;
    var post = this.http.post(url+"/api/services/app/UserCredits/EnableTopup",null);
    return post;
  }
}



export interface UserHistory {
  message: string;
  value:number;
  createdDate: Date;
}
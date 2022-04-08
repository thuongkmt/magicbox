import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import {  ListResultDtoOfPermissionDto, API_BASE_URL } from './service-proxies';
import { blobToText, throwException } from './service-base';


@Injectable()
export class PaymentServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    disablePayment(): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/PaymentDeviceService/DisablePayments";
        url_ = url_.replace(/[?&]$/, "");
        console.log(url_);
        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("post", url_, options_).flatMap((response_: any) => {
            return this.processDisablePayment(response_);
        }).catch((response_: any) => {
            console.log(response_);
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processDisablePayment(<any>response_);
                } catch (e) {
                    return <Observable<any>><any>Observable.throw(e);
                }
            } else
                return <Observable<any>><any>Observable.throw(response_);
        });

       
    }

    protected processDisablePayment(response: HttpResponseBase): Observable<any> {
        console.log(response);
        console.log(response.status);
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return Observable.of<any>(<any>null);
            });
        } else if (status === 401) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("A server error occurred.", status, _responseText, _headers);
            });
        } else if (status === 403) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("A server error occurred.", status, _responseText, _headers);
            });
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Observable.of<any>(<any>null);
    }
   
    enablePayment(): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/PaymentDeviceService/EnablePayments";
        url_ = url_.replace(/[?&]$/, "");
        console.log(url_);
        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("post", url_, options_).flatMap((response_: any) => {
            console.log('haha');
            return this.processEnablePayment(response_);
        }).catch((response_: any) => {
            console.log(response_);
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processEnablePayment(<any>response_);
                } catch (e) {
                    return <Observable<any>><any>Observable.throw(e);
                }
            } else
                return <Observable<any>><any>Observable.throw(response_);
        });

       
    }

    protected processEnablePayment(response: HttpResponseBase): Observable<any> {
        console.log(response);
        console.log(response.status);
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return Observable.of<any>(<any>null);
            });
        } else if (status === 401) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("A server error occurred.", status, _responseText, _headers);
            });
        } else if (status === 403) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("A server error occurred.", status, _responseText, _headers);
            });
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Observable.of<any>(<any>null);
    }

    // getMdbLog(): Observable<string> {
    //     let url_ = this.baseUrl + "/api/services/app/CommonService/ReadLog";
     

    //     url_ = url_.replace(/[?&]$/, "");
    //     const content_ = JSON.stringify({
    //         "logType": "information",
    //         "date":new Date()
    //       });
    //     let options_ : any = {
    //         body:content_,
    //         observe: "response",
    //         responseType: "blob",
    //         headers: new HttpHeaders({
    //             "Content-Type": "application/json", 
    //             "Accept": "application/json"
    //         })
    //     };

    //     return this.http.request("post", url_, options_).flatMap((response_ : any) => {
    //         return this.processReadMdbLog(response_);
    //     }).catch((response_: any) => {
    //         if (response_ instanceof HttpResponseBase) {
    //             try {
    //                 return this.processReadMdbLog(<any>response_);
    //             } catch (e) {
    //                 return <Observable<string>><any>Observable.throw(e);
    //             }
    //         } else
    //             return <Observable<string>><any>Observable.throw(response_);
    //     });
    // }

    // protected processReadMdbLog(response: HttpResponseBase): Observable<string> {
    //     const status = response.status;
    //     const responseBlob = 
    //         response instanceof HttpResponse ? response.body : 
    //         (<any>response).error instanceof Blob ? (<any>response).error : undefined;

    //     let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
    //     if (status === 200) {
    //         return blobToText(responseBlob).flatMap(_responseText => {
    //         return Observable.of(_responseText);
    //         });
    //     } else if (status === 401) {
    //         return blobToText(responseBlob).flatMap(_responseText => {
    //         return throwException("A server error occurred.", status, _responseText, _headers);
    //         });
    //     } else if (status === 403) {
    //         return blobToText(responseBlob).flatMap(_responseText => {
    //         return throwException("A server error occurred.", status, _responseText, _headers);
    //         });
    //     } else if (status !== 200 && status !== 204) {
    //         return blobToText(responseBlob).flatMap(_responseText => {
    //         return throwException("An unexpected server error occurred.", status, _responseText, _headers);
    //         });
    //     }
    //     return Observable.of<string>(<any>null);
    // }

}


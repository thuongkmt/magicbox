import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import {  ListResultDtoOfPermissionDto, API_BASE_URL } from './service-proxies';
import { blobToText, throwException } from './service-base';


@Injectable()
export class CommonServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

   

    getMdbLog(): Observable<string> {
        let url_ = this.baseUrl + "/api/services/app/CommonService/ReadLog";
     

        url_ = url_.replace(/[?&]$/, "");
        const content_ = JSON.stringify({
            "logType": "information",
            "date":new Date()
          });
        let options_ : any = {
            body:content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json", 
                "Accept": "application/json"
            })
        };

        return this.http.request("post", url_, options_).flatMap((response_ : any) => {
            return this.processReadMdbLog(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processReadMdbLog(<any>response_);
                } catch (e) {
                    return <Observable<string>><any>Observable.throw(e);
                }
            } else
                return <Observable<string>><any>Observable.throw(response_);
        });
    }

    protected processReadMdbLog(response: HttpResponseBase): Observable<string> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
            return Observable.of(_responseText);
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
        return Observable.of<string>(<any>null);
    }

    getComPorts(): Observable<GetComPortDtoList> {
        let url_ = this.baseUrl + "/api/services/app/CommonService/GetComPorts";
     
        url_ = url_.replace(/[?&]$/, "");
        
        let options_ : any = { 
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json", 
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).flatMap((response_ : any) => {
            return this.processGetComPorts(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetComPorts(<any>response_);
                } catch (e) {
                    return <Observable<GetComPortDtoList>><any>Observable.throw(e);
                }
            } else
                return <Observable<GetComPortDtoList>><any>Observable.throw(response_);
        });
    }

    protected processGetComPorts(response: HttpResponseBase): Observable<GetComPortDtoList> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? GetComPortDtoList.fromJS(resultData200) : new GetComPortDtoList();
                console.log(result200);
                return Observable.of(result200);
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
        return Observable.of<GetComPortDtoList>(<any>null);
    }

}


export class GetComPortDto {
    port:string;
    isSelected:boolean;

    
    init(data?: any) {
        if (data) {
            this.port = data["port"];
            this.isSelected = data["isSelected"];
          
        }
    }

    static fromJS(data: any): GetComPortDto {
        data = typeof data === 'object' ? data : {};
        let result = new GetComPortDto();
        result.init(data);
        return result;
    }
}

export class GetComPortDtoList{
    items: GetComPortDto[] | undefined;

    init(data?: any) {
        if (data) {
            if (data.constructor === Array) {
                this.items = [];
                for (let item of data)
                    this.items.push(GetComPortDto.fromJS(item));
            }
        }
    }
    static fromJS(data: any): GetComPortDtoList {
        data = typeof data === 'object' ? data : {};
        //console.log(data);
        let result = new GetComPortDtoList();
        result.init(data);
        return result;
    }
}


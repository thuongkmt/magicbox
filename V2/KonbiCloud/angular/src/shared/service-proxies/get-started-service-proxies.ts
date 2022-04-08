import * as moment from 'moment';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { blobToText, throwException } from './service-base';
import {  ListResultDtoOfPermissionDto, API_BASE_URL } from './service-proxies';

@Injectable()
export class GetStartedServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    getGetStartedStatus(): Observable<GetStartedDataOutput[]> {
        let url_ = this.baseUrl + "/api/services/app/GetStarted/getGetStartedStatus";
        url_ = url_.replace(/[?&]$/, "");
        //const content_ = JSON.stringify(input);
        let options_: any = {
            //body: content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).flatMap((response_: any) => {
            return this.processCreateOrUpdate(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processCreateOrUpdate(<any>response_);
                } catch (e) {
                    return <Observable<GetStartedDataOutput[]>><any>Observable.throw(e);
                }
            } else
                return <Observable<GetStartedDataOutput[]>><any>Observable.throw(response_);
        });
    }

    protected processCreateOrUpdate(response: HttpResponseBase): Observable<GetStartedDataOutput[]> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? resultData200 : new Array<GetStartedDataOutput>();
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
        return Observable.of<GetStartedDataOutput[]>(<any>null);
    }
}

export class GetStartedDataOutput implements IGetStartedDataOutput {

    stepId: number;
    stepName: string;
    stepTitle: string;
    stepSubTitle: string;
    stepActionUrl: string;
    stepDoneFlg: number;

    constructor(data?: IGetStartedDataOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.stepId = data["stepId"];
            this.stepName = data["stepName"];
            this.stepTitle = data["stepTitle"];
            this.stepSubTitle = data["stepSubTitle"];
            this.stepActionUrl = data["stepActionUrl"];
            this.stepDoneFlg = data["stepDoneFlg"];
        }
    }

    static fromJS(data: any): GetStartedDataOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetStartedDataOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["stepId"] = this.stepId;
        data["stepName"] = this.stepName;
        data["stepTitle"] = this.stepTitle;
        data["stepSubTitle"] = this.stepSubTitle;
        data["stepActionUrl"] = this.stepActionUrl;
        data["stepDoneFlg"] = this.stepDoneFlg;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new GetStartedDataOutput();
        result.init(json);
        return result;
    }
}

export interface IGetStartedDataOutput {
    stepId: number;
    stepName: string;
    stepTitle: string;
    stepSubTitle: string;
    stepActionUrl: string;
    stepDoneFlg: number;
}
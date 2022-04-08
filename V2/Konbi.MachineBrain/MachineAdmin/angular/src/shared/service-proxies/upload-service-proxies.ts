import { blobToText, throwException } from './service-base';
import { API_BASE_URL } from './service-proxies';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { Http, Headers, RequestOptions } from '@angular/http';
import { HttpClient, HttpParams, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';

@Injectable()
export class UploadServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    upload(files, params) {
        const url_ = this.baseUrl + '/file/upload';
        return this.http.post(url_, files, { params: params })
            .map(response => { return response })
            .catch(error => Observable.throw(error));
    }
}

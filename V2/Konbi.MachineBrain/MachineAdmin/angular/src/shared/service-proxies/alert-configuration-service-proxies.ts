import { Observable } from "rxjs/internal/Observable";
import { HttpClient, HttpHeaders, HttpResponseBase, HttpResponse } from "@angular/common/http";
import { Optional, Injectable, Inject } from "@angular/core";
import { AlertConfiguration } from "@shared/service-proxies/service-proxies";
import { blobToText, throwException } from "@shared/service-proxies/service-base";
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { throwError as _observableThrow, of as _observableOf } from 'rxjs';
import * as ApiServiceProxies from './service-proxies';

export enum MachineStatus {
    NONE = -1,
    IDLE,
    TRANSACTION_START,
    TRANSACTION_FAILED,
    TRANSACTION_DONE,
    STOPSALE,
    MANUAL_STOPSALE,
    TRANSACTION_DOOR_CLOSED,
    VALIDATE_CARD_FAILED,
    STOPSALE_DUE_TO_PAYMENT,
    TRANSACTION_WAITTING_FOR_PAYMENT,
    FAIL_TO_OPEN_THE_DOOR,
    UNSTABLE_TAGS_DIAGNOSTIC,
    UNSTABLE_TAGS_DIAGNOSTIC_TRACING,
    UNLOADING_PRODUCT
}

@Injectable()
export class AlertSettingAppServicesServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(ApiServiceProxies.API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @param machineID (optional) 
     * @return Success
     */
    getAlertConfiguration(machineID: string | null | undefined): Observable<AlertConfiguration> {
        let url_ = this.baseUrl + "/api/services/app/AlertSetting/GetAlertConfiguration?";
        if (machineID !== undefined)
            url_ += "machineID=" + encodeURIComponent("" + machineID) + "&"; 
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetAlertConfiguration(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAlertConfiguration(<any>response_);
                } catch (e) {
                    return <Observable<AlertConfiguration>><any>_observableThrow(e);
                }
            } else
                return <Observable<AlertConfiguration>><any>_observableThrow(response_);
        }));
    }

    protected processGetAlertConfiguration(response: HttpResponseBase): Observable<AlertConfiguration> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? AlertConfiguration.fromJS(resultData200) : new AlertConfiguration();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<AlertConfiguration>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    updateAlertConfig(input: AlertConfiguration | null | undefined): Observable<AlertConfiguration> {
        let url_ = this.baseUrl + "/api/services/app/AlertSetting/CreateOrEdit";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(input);

        let options_ : any = {
            body: content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json", 
                "Accept": "application/json"
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processUpdateAlertConfig(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdateAlertConfig(<any>response_);
                } catch (e) {
                    return <Observable<AlertConfiguration>><any>_observableThrow(e);
                }
            } else
                return <Observable<AlertConfiguration>><any>_observableThrow(response_);
        }));
    }

    protected processUpdateAlertConfig(response: HttpResponseBase): Observable<AlertConfiguration> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? AlertConfiguration.fromJS(resultData200) : new AlertConfiguration();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<AlertConfiguration>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    updateMachineStatus(input: MachineStatus | null | undefined): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/StopSale/ChangeMachineStatus";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(input);

        let options_ : any = {
            body: content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json", 
                "Accept": "application/json"
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processUpdateMachineStatus(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdateMachineStatus(<any>response_);
                } catch (e) {
                    return <Observable<void>><any>_observableThrow(e);
                }
            } else
                return <Observable<void>><any>_observableThrow(response_);
        }));
    }

    protected processUpdateMachineStatus(response: HttpResponseBase): Observable<void> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? result200 : new Observable<String[]>();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<void>(<any>null);
    }
}
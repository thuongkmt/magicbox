import 'rxjs/add/operator/finally';
import 'rxjs/add/observable/fromPromise';
import 'rxjs/add/observable/of';
import 'rxjs/add/observable/throw';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/toPromise';
import 'rxjs/add/operator/mergeMap';
import 'rxjs/add/operator/catch';
import * as ApiServiceProxies from './service-proxies';

import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import * as moment from 'moment';

@Injectable()
export class TemperatureLogsServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(ApiServiceProxies.API_BASE_URL) baseUrl?: string) {        
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @param filter (optional)
     * @return Success
     */
    getTemperatureLogs
    (
        filter: string | null | undefined,
        dateFilter : string | null | undefined
    ): Observable<ListResultDtoOfTemperatureListDto> {
        let url_ = this.baseUrl + "/api/services/app/TemperatureLogs/GetTemperatureLogs?";
        if (filter !== undefined) {
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&";
        }

        if(dateFilter !== undefined && dateFilter != null){
            url_ += "DateFilter=" + encodeURIComponent("" + dateFilter) + "&"; 
        }

        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetTemperatureLogs(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetTemperatureLogs(<any>response_);
                } catch (e) {
                    return <Observable<ListResultDtoOfTemperatureListDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<ListResultDtoOfTemperatureListDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetTemperatureLogs(response: HttpResponseBase): Observable<ListResultDtoOfTemperatureListDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? ListResultDtoOfTemperatureListDto.fromJS(resultData200) : new ListResultDtoOfTemperatureListDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<ListResultDtoOfTemperatureListDto>(<any>null);
    }
}

export class ListResultDtoOfTemperatureListDto implements IListResultDtoOfTemperatureListDto {
    items!: TemperatureListDto[] | undefined;

    constructor(data?: IListResultDtoOfTemperatureListDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["items"] && data["items"].constructor === Array) {
                this.items = [];
                for (let item of data["items"])
                    this.items.push(TemperatureListDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfTemperatureListDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfTemperatureListDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.items && this.items.constructor === Array) {
            data["items"] = [];
            for (let item of this.items)
                data["items"].push(item.toJSON());
        }
        return data;
    }
}

export interface IListResultDtoOfTemperatureListDto {
    items: TemperatureListDto[] | undefined;
}

export class TemperatureListDto implements ITemperatureListDto {
    machineId!: string | undefined;
    machineName!: string | undefined;
    temperature!: number | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: Date | undefined;
    creatorUserId!: number | undefined;
    id!: number | undefined;

    constructor(data?: ITemperatureListDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.machineId = data["machineId"];
            this.machineName = data["machineName"];
            this.temperature = data["temperature"];
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? new Date(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): TemperatureListDto {
        data = typeof data === 'object' ? data : {};
        let result = new TemperatureListDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["machineId"] = this.machineId;
        data["machineName"] = this.machineName;
        data["temperature"] = this.temperature;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface ITemperatureListDto {
    machineId: string | undefined;
    machineName: string | undefined;
    temperature: number | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: Date | undefined;
    creatorUserId: number | undefined;
    id: number | undefined;
}

function blobToText(blob: any): Observable<string> {
    return new Observable<string>((observer: any) => {
        if (!blob) {
            observer.next("");
            observer.complete();
        } else {
            let reader = new FileReader(); 
            reader.onload = function() { 
                observer.next(this.result);
                observer.complete();
            }
            reader.readAsText(blob); 
        }
    });
}

function throwException(message: string, status: number, response: string, headers: { [key: string]: any; }, result?: any): Observable<any> {
    if(result !== null && result !== undefined)
        return Observable.throw(result);
    else
        //return Observable.throw(new ApiServiceProxies.SwaggerException(message, status, response, headers, null));
        throwException(message, status, response, headers);
}
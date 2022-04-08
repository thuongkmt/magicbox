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
import { DiscDto } from './service-proxies';

import * as moment from 'moment';


@Injectable()
export class EmployeeServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(ApiServiceProxies.API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @return Success
     */
    getAllEmployees(
        nameFilter: string | null | undefined,
        sorting: string | null | undefined,
        maxResultCount: number | null | undefined,
        skipCount: number | null | undefined
        ): Observable<PagedResultDtoOfEmployeeDto> {
        let url_ = this.baseUrl + "/api/services/app/Employee/GetAllEmployee?";
        if (nameFilter !== undefined)
            url_ += "NameFilter=" + encodeURIComponent("" + nameFilter) + "&";
        if (sorting !== undefined)
            url_ += "Sorting=" + encodeURIComponent("" + sorting) + "&";
        if (maxResultCount !== undefined)
            url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&";
        if (skipCount !== undefined)
            url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&";

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
            return this.processGetAllEmployees(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllEmployees(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfEmployeeDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<PagedResultDtoOfEmployeeDto>><any>Observable.throw(response_);
        });
    }

    protected processGetAllEmployees(response: HttpResponseBase): Observable<PagedResultDtoOfEmployeeDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? PagedResultDtoOfEmployeeDto.fromJS(resultData200) : new PagedResultDtoOfEmployeeDto();
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
        return Observable.of<PagedResultDtoOfEmployeeDto>(<any>null);
    }
}


export class EmployeeDto {
    id: string;
    name: string;
    cardId: string;
    quota: number;
    period: string;
    ordered: number;
    quotaCash: boolean;
    creationTime: string;

    constructor(data?: EmployeeDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.id = data["id"];
            this.name = data["name"];
            this.cardId = data["cardId"];
            this.quota = data["quota"];
            this.period = data["period"];
            this.ordered = data["ordered"];
            this.quotaCash = data["quotaCash"];
            this.creationTime = data["creationTime"];
        }
    }

    static fromJS(data: any): EmployeeDto {
        data = typeof data === 'object' ? data : {};
        let result = new EmployeeDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["name"] = this.name;
        data["cardId"] = this.cardId;
        data["quota"] = this.quota;
        data["period"] = this.period;
        data["ordered"] = this.ordered;
        data["quotaCash"] = this.quotaCash;
        data["creationTime"] = this.creationTime;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new EmployeeDto();
        result.init(json);
        return result;
    }
}

export class PagedResultDtoOfEmployeeDto{
    totalCount: number | undefined;
    items: EmployeeDto[] | undefined;

    constructor(data?: PagedResultDtoOfEmployeeDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.totalCount = data["totalCount"];
            if (data["items"] && data["items"].constructor === Array) {
                this.items = [];
                for (let item of data["items"])
                    this.items.push(EmployeeDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfEmployeeDto {
        data = typeof data === 'object' ? data : {};
        //console.log(data);
        let result = new PagedResultDtoOfEmployeeDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["totalCount"] = this.totalCount;
        if (this.items && this.items.constructor === Array) {
            data["items"] = [];
            for (let item of this.items)
                data["items"].push(item.toJSON());
        }
        return data; 
    }
 
    clone() {
        const json = this.toJSON();
        let result = new PagedResultDtoOfEmployeeDto();
        result.init(json);
        return result;
    }
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
        return Observable.throw(new ApiServiceProxies.SwaggerException(message, status, response, headers, null));
}

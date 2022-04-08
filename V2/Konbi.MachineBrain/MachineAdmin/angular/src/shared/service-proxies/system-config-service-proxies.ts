import { ListResultDtoOfPermissionDto, API_BASE_URL } from './service-proxies';
import { PlateCategoryDto } from './plate-category-service-proxies'
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { blobToText, throwException } from './service-base';
import * as moment from 'moment';

@Injectable()
export class SystemConfigServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor( @Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @input (optional) 
     * @return Success
     */
    update(input: SystemConfigDto | null | undefined): Observable<SystemConfigDto> {
        let url_ = this.baseUrl + "/api/services/app/SystemConfigService/Update";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(input);

        let options_: any = {
            body: content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("put", url_, options_).flatMap((response_: any) => {
            return this.processUpdate(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdate(<any>response_);
                } catch (e) {
                    return <Observable<SystemConfigDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<SystemConfigDto>><any>Observable.throw(response_);
        });
    }

    protected processUpdate(response: HttpResponseBase): Observable<SystemConfigDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? SystemConfigDto.fromJS(resultData200) : new SystemConfigDto();
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
        return Observable.of<SystemConfigDto>(<any>null);
    }

    /**
     * @return Success
     */
    getAllByName(
        name: string | null | undefined
    ): Observable<PagedResultDtoOfSystemConfigDto> {
        let url_ = this.baseUrl + '/api/services/app/SystemConfigService/GetAll?';
        if (name !== undefined)
            url_ += "Name=" + encodeURIComponent("" + name) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).flatMap((response_: any) => {
            return this.processGetAll(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAll(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfSystemConfigDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<PagedResultDtoOfSystemConfigDto>><any>Observable.throw(response_);
        });
    }

    /**
     * @return Success
     */
    getAll(): Observable<PagedResultDtoOfSystemConfigDto> {
        console.log("call here 1");
        let url_ = this.baseUrl + "/api/services/app/SystemConfigService/GetAll?";
        // if (skipCount === undefined || skipCount === null)
        //     throw new Error("The parameter 'skipCount' must be defined and cannot be null.");
        // else
        //     url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&";
        // if (maxResultCount === undefined || maxResultCount === null)
        //     throw new Error("The parameter 'maxResultCount' must be defined and cannot be null.");
        // else
        //     url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).flatMap((response_: any) => {
            return this.processGetAll(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAll(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfSystemConfigDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<PagedResultDtoOfSystemConfigDto>><any>Observable.throw(response_);
        });
    }

    protected processGetAll(response: HttpResponseBase): Observable<PagedResultDtoOfSystemConfigDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PagedResultDtoOfSystemConfigDto.fromJS(resultData200) : new PagedResultDtoOfSystemConfigDto();
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
        return Observable.of<PagedResultDtoOfSystemConfigDto>(<any>null);
    }
}

export class SystemConfigDto implements ISystemConfigDto {
    name: string;
    value: string;
    //tenantId: number;

    constructor(data?: ISystemConfigDto) { 
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.value = data["value"];
            //this.tenantId = data["tenantId"];
        }
    }

    static fromJS(data: any): SystemConfigDto {
        data = typeof data === 'object' ? data : {};
        let result = new SystemConfigDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["value"] = this.value;
        //data["tenantId"] = this.tenantId;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new SystemConfigDto();
        result.init(json);
        return result;
    }
}

export interface ISystemConfigDto {
    name: string;
    value: string;
    //tenantId: number;
}


export class PagedResultDtoOfSystemConfigDto implements IPagedResultDtoOfSystemConfigDto {
    totalCount: number | undefined;
    items: SystemConfigDto[] | undefined;

    constructor(data?: IPagedResultDtoOfSystemConfigDto) {
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
                    this.items.push(SystemConfigDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfSystemConfigDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfSystemConfigDto();
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
        let result = new PagedResultDtoOfSystemConfigDto();
        result.init(json);
        return result;
    }
}

export interface IPagedResultDtoOfSystemConfigDto {
    totalCount: number | undefined;
    items: SystemConfigDto[] | undefined;
}

// end system service

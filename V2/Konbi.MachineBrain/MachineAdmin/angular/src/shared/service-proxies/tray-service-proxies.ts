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
//import { DiscDto } from './service-proxies';

import * as moment from 'moment';


@Injectable()
export class TrayServiceProxy {
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
    getAllTrays(
        nameFilter: string | null | undefined,
        codeFilter: string | null | undefined,
        sorting: string | null | undefined,
        maxResultCount: number | null | undefined,
        skipCount: number | null | undefined
        ): Observable<PagedResultDtoOfTrayDto> {
        let url_ = this.baseUrl + "/api/services/app/Tray/GetAllTrays?";
        if (nameFilter !== undefined)
            url_ += "NameFilter=" + encodeURIComponent("" + nameFilter) + "&";
        if (codeFilter !== undefined)
            url_ += "CodeFilter=" + encodeURIComponent("" + codeFilter) + "&";
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
            return this.processGetAllTrays(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllTrays(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfTrayDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<PagedResultDtoOfTrayDto>><any>Observable.throw(response_);
        });
    }

    protected processGetAllTrays(response: HttpResponseBase): Observable<PagedResultDtoOfTrayDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? PagedResultDtoOfTrayDto.fromJS(resultData200) : new PagedResultDtoOfTrayDto();
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
        return Observable.of<PagedResultDtoOfTrayDto>(<any>null);
    }

    createOrEditTray(input: TrayDto | null | undefined): Observable<TrayDto> {
        let url_ = this.baseUrl + "/api/services/app/Tray/CreateOrEditTray";
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

        return this.http.request("post", url_, options_).flatMap((response_: any) => {
            return this.processCreateOrEditTray(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processCreateOrEditTray(<any>response_);
                } catch (e) {
                    return <Observable<TrayDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<TrayDto>><any>Observable.throw(response_);
        });
    }

    protected processCreateOrEditTray(response: HttpResponseBase): Observable<TrayDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? TrayDto.fromJS(resultData200) : new TrayDto();
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
        return Observable.of<TrayDto>(<any>null);
    }
    deleteTray(id: string): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/Tray/Delete?";
        if (id === undefined || id === null)
            throw new Error("The parameter 'id' must be defined and cannot be null.");
        else
            url_ += "Id=" + encodeURIComponent("" + id) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
            })
        };

        return this.http.request("delete", url_, options_).flatMap((response_: any) => {
            return this.processDeleteTray(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processDeleteTray(<any>response_);
                } catch (e) {
                    return <Observable<void>><any>Observable.throw(e);
                }
            } else
                return <Observable<void>><any>Observable.throw(response_);
        });
    }

    protected processDeleteTray(response: HttpResponseBase): Observable<void> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return Observable.of<void>(<any>null);
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
        return Observable.of<void>(<any>null);
    }
    syncTray(): Observable<any> {
        let url_ = this.baseUrl + "/api/services/app/Tray/SyncTrayData";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("post", url_, options_).flatMap((response_: any) => {
            return this.processSyncData(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processSyncData(<any>response_);
                } catch (e) {
                    return <Observable<any>><any>Observable.throw(e);
                }
            } else
                return <Observable<any>><any>Observable.throw(response_);
        });
    }

    protected processSyncData(response: HttpResponseBase): Observable<any> {
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
}


export class TrayDto {
    id: string | undefined;
    name: string;
    code: string;
    message: string;

    constructor(data?: TrayDto) {
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
            this.code = data["code"];
            this.message = data["message"];
        }
    }

    static fromJS(data: any): TrayDto {
        data = typeof data === 'object' ? data : {};
        let result = new TrayDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["name"] = this.name;
        data["code"] = this.code;
        data["message"] = this.message;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new TrayDto();
        result.init(json);
        return result;
    }
}

export class PagedResultDtoOfTrayDto{
    totalCount: number | undefined;
    items: TrayDto[] | undefined;

    constructor(data?: PagedResultDtoOfTrayDto) {
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
                    this.items.push(TrayDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfTrayDto {
        data = typeof data === 'object' ? data : {};
        //console.log(data);
        let result = new PagedResultDtoOfTrayDto();
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
        let result = new PagedResultDtoOfTrayDto();
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

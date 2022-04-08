import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { blobToText, throwException } from './service-base';
import { EntityDto, API_BASE_URL } from './service-proxies';

//Machine
@Injectable()
export class MachineLoadoutServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    getMachineLoadout(id: string): Observable<LoadoutDto> {
        let url_ = this.baseUrl + "/api/services/app/MachineLoadout/GetLoadout?";
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
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).flatMap((response_: any) => {
            return this.processGetMachineLoadout(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetMachineLoadout(<any>response_);
                } catch (e) {
                    return <Observable<LoadoutDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<LoadoutDto>><any>Observable.throw(response_);
        });
    }

    protected processGetMachineLoadout(response: HttpResponseBase): Observable<LoadoutDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? LoadoutDto.fromJS(resultData200) : new LoadoutDto();
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
        return Observable.of<LoadoutDto>(<any>null);
    }

    updateLoadout(input: UpdateLoadoutDto | null | undefined): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/MachineLoadout/UpdateLoadout";
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
            return this.processUpdateLoadout(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdateLoadout(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>Observable.throw(e);
                }
            } else
                return <Observable<boolean>><any>Observable.throw(response_);
        });
    }

    protected processUpdateLoadout(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? true : false;
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
        return Observable.of<boolean>(<any>null);
    }

    updateLoadoutItem(input: LoadoutItem): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/MachineLoadout/UpdateLoadoutItem";
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
            return this.processUpdateLoadoutItem(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdateLoadoutItem(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>Observable.throw(e);
                }
            } else
                return <Observable<boolean>><any>Observable.throw(response_);
        });
    }

    protected processUpdateLoadoutItem(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? resultData200 : false;
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
        return Observable.of<boolean>(<any>null);
    }

    getRestockSession(id: string): Observable<RestockSession> {
        let url_ = this.baseUrl + "/api/services/app/Restock/GetRestockSession?";
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
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).flatMap((response_: any) => {
            return this.processGetRestockSession(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetRestockSession(<any>response_);
                } catch (e) {
                    return <Observable<RestockSession>><any>Observable.throw(e);
                }
            } else
                return <Observable<RestockSession>><any>Observable.throw(response_);
        });
    }

    protected processGetRestockSession(response: HttpResponseBase): Observable<RestockSession> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? RestockSession.fromJS(resultData200) : new RestockSession();
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
        return Observable.of<RestockSession>(<any>null);
    }

    startRestock(input: EntityDto): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/Restock/StartRestock";

        let options_ : any = {
            body: JSON.stringify(input),
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processStartRestock(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processStartRestock(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>_observableThrow(e);
                }
            } else
                return <Observable<boolean>><any>_observableThrow(response_);
        }));
    }

    protected processStartRestock(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? true : false;
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<boolean>(<any>null);
    }

    endRestock(input: EntityDto): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/Restock/EndRestock";

        let options_ : any = {
            body: JSON.stringify(input),
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processEndRestock(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processStartRestock(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>_observableThrow(e);
                }
            } else
                return <Observable<boolean>><any>_observableThrow(response_);
        }));
    }

    protected processEndRestock(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? true : false;
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<boolean>(<any>null);
    }
}

export class LoadoutDto {
    machineId: string;
    machineName: string;
    isOnline: boolean;
    loadoutList: LoadoutList[];
    
    constructor(data?: LoadoutDto) {
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
            this.isOnline = data["isOnline"];
            this.loadoutList = data["loadoutList"];
        }
    }

    static fromJS(data: any): LoadoutDto {
        data = typeof data === 'object' ? data : {};
        let result = new LoadoutDto();
        result.init(data);
        return result;
    }
}
export class LoadoutList
{
    index: number;
    loadouts: LoadoutItem[];
    checkAll: boolean;
}

export class LoadoutItem
{
    id: string;
    machineId: string;
    productId: string;
    productName: string;
    imageUrl: string;
    price: number;
    quantity: number;
    capacity: number;
    itemLocation: string;
    isChecked: boolean;
    isDisable: boolean;

    hasProduct() {
        return this.productId != undefined && this.productId != null;
    }
}
export class UpdateLoadoutDto
{
    machineId: string;
    loadouts: LoadoutItem[];
}

export class RestockSession
{
    id: string;
    machineId: string;
    productId: string;
    total: number;
    leftover: number;
    sold: number;
    isClose: boolean;
    isInprogress: boolean;

    constructor(data?: RestockSession) {
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
            this.machineId = data["machineId"];
            this.productId = data["productId"];
            this.total = data["total"];
            this.leftover = data["leftover"];
            this.sold = data["sold"];
            this.isClose = data["isClose"];
            this.isInprogress = data["isInprogress"];
        }
    }

    static fromJS(data: any): RestockSession {
        data = typeof data === 'object' ? data : {};
        let result = new RestockSession();
        result.init(data);
        return result;
    }
}
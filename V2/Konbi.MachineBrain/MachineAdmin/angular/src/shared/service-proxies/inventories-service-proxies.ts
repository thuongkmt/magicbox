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
import { FileDto, ProductDto } from '@shared/service-proxies/service-proxies';

import * as moment from 'moment';


@Injectable()
export class InventoriesServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(ApiServiceProxies.API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    newTopup(): Observable<TopupDto> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/NewTopup";
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processNewTopup(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processNewTopup(<any>response_);
                } catch (e) {
                    return <Observable<TopupDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<TopupDto>><any>_observableThrow(response_);
        }));
    }

    protected processNewTopup(response: HttpResponseBase): Observable<TopupDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? TopupDto.fromJS(resultData200) : new TopupDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<TopupDto>(<any>null);
    }

    /**
     * @param total (optional) 
     * @return Success
     */
    endTopup(total: number | null | undefined): Observable<TopupDto> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/EndTopup?";
        if (total !== undefined)
            url_ += "total=" + encodeURIComponent("" + total) + "&"; 
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processEndTopup(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processEndTopup(<any>response_);
                } catch (e) {
                    return <Observable<TopupDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<TopupDto>><any>_observableThrow(response_);
        }));
    }

    protected processEndTopup(response: HttpResponseBase): Observable<TopupDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? TopupDto.fromJS(resultData200) : new TopupDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<TopupDto>(<any>null);
    }

    GetProductByTag(input: TagsInput): Observable<ProductTag[]> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/QueryProductByTag";
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
            return this.processGetProductByTag(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetProductByTag(<any>response_);
                } catch (e) {
                    return <Observable<ProductTag[]>><any>Observable.throw(e);
                }
            } else
                return <Observable<ProductTag[]>><any>Observable.throw(response_);
        });
    }

    protected processGetProductByTag(response: HttpResponseBase): Observable<ProductTag[]> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            if (resultData200 && resultData200.constructor === Array) {
                result200 = [];
                for (let item of resultData200)
                    result200.push(ProductTag.fromJS(item));
            }
            return _observableOf(result200);
            }));
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
        return Observable.of<ProductTag[]>(<any>null);
    }

    getCurrentTopup(): Observable<TopupDto> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/GetCurrentTopup";
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetCurrentTopup(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetCurrentTopup(<any>response_);
                } catch (e) {
                    return <Observable<TopupDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<TopupDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetCurrentTopup(response: HttpResponseBase): Observable<TopupDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? TopupDto.fromJS(resultData200) : new TopupDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<TopupDto>(<any>null);
    }

    /**
     * @return Success
     */
    getAllItems(): Observable<GetInventoryForWebApiDto[]> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/GetAllItems";
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetAllItems(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllItems(<any>response_);
                } catch (e) {
                    return <Observable<GetInventoryForWebApiDto[]>><any>_observableThrow(e);
                }
            } else
                return <Observable<GetInventoryForWebApiDto[]>><any>_observableThrow(response_);
        }));
    }

    protected processGetAllItems(response: HttpResponseBase): Observable<GetInventoryForWebApiDto[]> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            if (resultData200 && resultData200.constructor === Array) {
                result200 = [];
                for (let item of resultData200)
                    result200.push(GetInventoryForWebApiDto.fromJS(item));
            }
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<GetInventoryForWebApiDto[]>(<any>null);
    }

    /**
     * @return Success
     */
    unmapAll(): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/UnmapAll";
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processUnmapAll(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUnmapAll(<any>response_);
                } catch (e) {
                    return <Observable<void>><any>_observableThrow(e);
                }
            } else
                return <Observable<void>><any>_observableThrow(response_);
        }));
    }

    protected processUnmapAll(response: HttpResponseBase): Observable<void> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return _observableOf<void>(<any>null);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<void>(<any>null);
    }

    /**
     * @param filter (optional) 
     * @param tagIdFilter (optional) 
     * @param maxTrayLevelFilter (optional) 
     * @param minTrayLevelFilter (optional) 
     * @param maxPriceFilter (optional) 
     * @param minPriceFilter (optional) 
     * @param productNameFilter (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getAll(filter: string | null | undefined,
           tagIdFilter: string | null | undefined,
           maxTrayLevelFilter: number | null | undefined,
           minTrayLevelFilter: number | null | undefined,
           maxPriceFilter: number | null | undefined,
           minPriceFilter: number | null | undefined,
           productNameFilter: string | null | undefined,
           sorting: string | null | undefined,
           skipCount: number | null | undefined,
           maxResultCount: number | null | undefined): Observable<PagedResultDtoOfGetInventoryForViewDto> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/GetAll?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&"; 
        if (tagIdFilter !== undefined)
            url_ += "TagIdFilter=" + encodeURIComponent("" + tagIdFilter) + "&"; 
        if (maxTrayLevelFilter !== undefined)
            url_ += "MaxTrayLevelFilter=" + encodeURIComponent("" + maxTrayLevelFilter) + "&"; 
        if (minTrayLevelFilter !== undefined)
            url_ += "MinTrayLevelFilter=" + encodeURIComponent("" + minTrayLevelFilter) + "&"; 
        if (maxPriceFilter !== undefined)
            url_ += "MaxPriceFilter=" + encodeURIComponent("" + maxPriceFilter) + "&"; 
        if (minPriceFilter !== undefined)
            url_ += "MinPriceFilter=" + encodeURIComponent("" + minPriceFilter) + "&"; 
        if (productNameFilter !== undefined)
            url_ += "ProductNameFilter=" + encodeURIComponent("" + productNameFilter) + "&"; 
        if (sorting !== undefined)
            url_ += "Sorting=" + encodeURIComponent("" + sorting) + "&"; 
        if (skipCount !== undefined)
            url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&"; 
        if (maxResultCount !== undefined)
            url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&"; 
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetAll(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAll(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfGetInventoryForViewDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfGetInventoryForViewDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetAll(response: HttpResponseBase): Observable<PagedResultDtoOfGetInventoryForViewDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? PagedResultDtoOfGetInventoryForViewDto.fromJS(resultData200) : new PagedResultDtoOfGetInventoryForViewDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfGetInventoryForViewDto>(<any>null);
    }

    /**
     * @param id (optional) 
     * @return Success
     */
    getInventoryForView(id: string | null | undefined): Observable<GetInventoryForViewDto> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/GetInventoryForView?";
        if (id !== undefined)
            url_ += "id=" + encodeURIComponent("" + id) + "&"; 
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetInventoryForView(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetInventoryForView(<any>response_);
                } catch (e) {
                    return <Observable<GetInventoryForViewDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<GetInventoryForViewDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetInventoryForView(response: HttpResponseBase): Observable<GetInventoryForViewDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? GetInventoryForViewDto.fromJS(resultData200) : new GetInventoryForViewDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<GetInventoryForViewDto>(<any>null);
    }

    /**
     * @param id (optional) 
     * @return Success
     */
    getInventoryForEdit(id: string | null | undefined): Observable<GetInventoryForEditOutput> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/GetInventoryForEdit?";
        if (id !== undefined)
            url_ += "Id=" + encodeURIComponent("" + id) + "&"; 
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetInventoryForEdit(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetInventoryForEdit(<any>response_);
                } catch (e) {
                    return <Observable<GetInventoryForEditOutput>><any>_observableThrow(e);
                }
            } else
                return <Observable<GetInventoryForEditOutput>><any>_observableThrow(response_);
        }));
    }

    protected processGetInventoryForEdit(response: HttpResponseBase): Observable<GetInventoryForEditOutput> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? GetInventoryForEditOutput.fromJS(resultData200) : new GetInventoryForEditOutput();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<GetInventoryForEditOutput>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    createOrEdit(input: CreateOrEditInventoryDto | null | undefined): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/CreateOrEdit";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(input);

        let options_ : any = {
            body: content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json", 
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processCreateOrEdit(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processCreateOrEdit(<any>response_);
                } catch (e) {
                    return <Observable<void>><any>_observableThrow(e);
                }
            } else
                return <Observable<void>><any>_observableThrow(response_);
        }));
    }

    protected processCreateOrEdit(response: HttpResponseBase): Observable<void> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return _observableOf<void>(<any>null);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<void>(<any>null);
    }

    /**
     * @param items (optional) 
     * @return Success
     */
    topup(items: CreateOrEditInventoryDto[] | null | undefined): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/Topup";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(items);

        let options_ : any = {
            body: content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json", 
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processTopup(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processTopup(<any>response_);
                } catch (e) {
                    return <Observable<void>><any>_observableThrow(e);
                }
            } else
                return <Observable<void>><any>_observableThrow(response_);
        }));
    }

    protected processTopup(response: HttpResponseBase): Observable<void> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return _observableOf<void>(<any>null);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<void>(<any>null);
    }

    /**
     * @param id (optional) 
     * @return Success
     */
    delete(id: string | null | undefined): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/Delete?";
        if (id !== undefined)
            url_ += "Id=" + encodeURIComponent("" + id) + "&"; 
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
            })
        };

        return this.http.request("delete", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processDelete(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processDelete(<any>response_);
                } catch (e) {
                    return <Observable<void>><any>_observableThrow(e);
                }
            } else
                return <Observable<void>><any>_observableThrow(response_);
        }));
    }

    protected processDelete(response: HttpResponseBase): Observable<void> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return _observableOf<void>(<any>null);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<void>(<any>null);
    }

    /**
     * @param filter (optional) 
     * @param tagIdFilter (optional) 
     * @param maxTrayLevelFilter (optional) 
     * @param minTrayLevelFilter (optional) 
     * @param maxPriceFilter (optional) 
     * @param minPriceFilter (optional) 
     * @param productNameFilter (optional) 
     * @return Success
     */
    getInventoriesToExcel(filter: string | null | undefined, tagIdFilter: string | null | undefined, maxTrayLevelFilter: number | null | undefined, minTrayLevelFilter: number | null | undefined, maxPriceFilter: number | null | undefined, minPriceFilter: number | null | undefined, productNameFilter: string | null | undefined): Observable<FileDto> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/GetInventoriesToExcel?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&"; 
        if (tagIdFilter !== undefined)
            url_ += "TagIdFilter=" + encodeURIComponent("" + tagIdFilter) + "&"; 
        if (maxTrayLevelFilter !== undefined)
            url_ += "MaxTrayLevelFilter=" + encodeURIComponent("" + maxTrayLevelFilter) + "&"; 
        if (minTrayLevelFilter !== undefined)
            url_ += "MinTrayLevelFilter=" + encodeURIComponent("" + minTrayLevelFilter) + "&"; 
        if (maxPriceFilter !== undefined)
            url_ += "MaxPriceFilter=" + encodeURIComponent("" + maxPriceFilter) + "&"; 
        if (minPriceFilter !== undefined)
            url_ += "MinPriceFilter=" + encodeURIComponent("" + minPriceFilter) + "&"; 
        if (productNameFilter !== undefined)
            url_ += "ProductNameFilter=" + encodeURIComponent("" + productNameFilter) + "&"; 
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetInventoriesToExcel(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetInventoriesToExcel(<any>response_);
                } catch (e) {
                    return <Observable<FileDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<FileDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetInventoriesToExcel(response: HttpResponseBase): Observable<FileDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? FileDto.fromJS(resultData200) : new FileDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<FileDto>(<any>null);
    }

    /**
     * @param filter (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getAllProductForLookupTable(filter: string | null | undefined, sorting: string | null | undefined, skipCount: number | null | undefined, maxResultCount: number | null | undefined): Observable<PagedResultDtoOfProductLookupTableDto> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/GetAllProductForLookupTable?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&"; 
        if (sorting !== undefined)
            url_ += "Sorting=" + encodeURIComponent("" + sorting) + "&"; 
        if (skipCount !== undefined)
            url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&"; 
        if (maxResultCount !== undefined)
            url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&"; 
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetAllProductForLookupTable(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllProductForLookupTable(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfProductLookupTableDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfProductLookupTableDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetAllProductForLookupTable(response: HttpResponseBase): Observable<PagedResultDtoOfProductLookupTableDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? PagedResultDtoOfProductLookupTableDto.fromJS(resultData200) : new PagedResultDtoOfProductLookupTableDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfProductLookupTableDto>(<any>null);
    }

    /**
     * @return Success
     */
    getCurrentTopupInfo(): Observable<GetCurrentTopupDto> {
        let url_ = this.baseUrl + "/api/services/app/Inventories/GetCurrentTopupInfo";
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetCurrentTopupInfo(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetCurrentTopupInfo(<any>response_);
                } catch (e) {
                    return <Observable<GetCurrentTopupDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<GetCurrentTopupDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetCurrentTopupInfo(response: HttpResponseBase): Observable<GetCurrentTopupDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? GetCurrentTopupDto.fromJS(resultData200) : new GetCurrentTopupDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<GetCurrentTopupDto>(<any>null);
    }
}

export class PagedResultDtoOfProductLookupTableDto implements IPagedResultDtoOfProductLookupTableDto {
    totalCount!: number | undefined;
    items!: ProductLookupTableDto[] | undefined;

    constructor(data?: IPagedResultDtoOfProductLookupTableDto) {
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
                    this.items.push(ProductLookupTableDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfProductLookupTableDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfProductLookupTableDto();
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
}

export interface IPagedResultDtoOfProductLookupTableDto {
    totalCount: number | undefined;
    items: ProductLookupTableDto[] | undefined;
}

export class ProductLookupTableDto implements IProductLookupTableDto {
    id!: string | undefined;
    displayName!: string | undefined;

    constructor(data?: IProductLookupTableDto) {
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
            this.displayName = data["displayName"];
        }
    }

    static fromJS(data: any): ProductLookupTableDto {
        data = typeof data === 'object' ? data : {};
        let result = new ProductLookupTableDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["displayName"] = this.displayName;
        return data; 
    }
}

export interface IProductLookupTableDto {
    id: string | undefined;
    displayName: string | undefined;
}

export class TagsInput
{
    machineId: string;
    tags: any[]
}

export class ProductTag {
    productId: string;
    productName: string;
    tag: string;
    price: number;
    mapped: boolean;

    constructor(data?: ProductTag) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.productId = data["productId"];
            this.productName = data["productName"];
            this.tag = data["tag"];
            this.price = data["price"];
            this.mapped = data["mapped"];
        }
    }

    static fromJS(data: any): ProductTag {
        data = typeof data === 'object' ? data : {};
        let result = new ProductTag();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["productId"] = this.productId;
        data["productName"] = this.productName;
        data["tag"] = this.tag;
        data["price"] = this.price;
        data["mapped"] = this.mapped;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new ProductTag();
        result.init(json);
        return result;
    }
}

export class GetCurrentTopupDto implements IGetCurrentTopupDto {
    startTime!: moment.Moment | undefined;
    total!: number | undefined;
    sold!: number | undefined;
    leftOver!: number | undefined;

    constructor(data?: IGetCurrentTopupDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.startTime = data["startTime"] ? moment(data["startTime"].toString()) : <any>undefined;
            this.total = data["total"];
            this.sold = data["sold"];
            this.leftOver = data["leftOver"];
        }
    }

    static fromJS(data: any): GetCurrentTopupDto {
        data = typeof data === 'object' ? data : {};
        let result = new GetCurrentTopupDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["startTime"] = this.startTime ? this.startTime.toISOString() : <any>undefined;
        data["total"] = this.total;
        data["sold"] = this.sold;
        data["leftOver"] = this.leftOver;
        return data; 
    }
}

export interface IGetCurrentTopupDto {
    startTime: moment.Moment | undefined;
    total: number | undefined;
    sold: number | undefined;
    leftOver: number | undefined;
}

export class CreateOrEditInventoryDto implements ICreateOrEditInventoryDto {
    tagId!: string;
    trayLevel!: number | undefined;
    price!: number | undefined;
    productId!: string | undefined;
    id!: string | undefined;

    constructor(data?: ICreateOrEditInventoryDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tagId = data["tagId"];
            this.trayLevel = data["trayLevel"];
            this.price = data["price"];
            this.productId = data["productId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CreateOrEditInventoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrEditInventoryDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tagId"] = this.tagId;
        data["trayLevel"] = this.trayLevel;
        data["price"] = this.price;
        data["productId"] = this.productId;
        data["id"] = this.id;
        return data; 
    }
}

export interface ICreateOrEditInventoryDto {
    tagId: string;
    trayLevel: number | undefined;
    price: number | undefined;
    productId: string | undefined;
    id: string | undefined;
}

export class GetInventoryForEditOutput implements IGetInventoryForEditOutput {
    inventory!: CreateOrEditInventoryDto | undefined;
    productName!: string | undefined;

    constructor(data?: IGetInventoryForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.inventory = data["inventory"] ? CreateOrEditInventoryDto.fromJS(data["inventory"]) : <any>undefined;
            this.productName = data["productName"];
        }
    }

    static fromJS(data: any): GetInventoryForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetInventoryForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["inventory"] = this.inventory ? this.inventory.toJSON() : <any>undefined;
        data["productName"] = this.productName;
        return data; 
    }
}

export interface IGetInventoryForEditOutput {
    inventory: CreateOrEditInventoryDto | undefined;
    productName: string | undefined;
}

export class PagedResultDtoOfGetInventoryForViewDto implements IPagedResultDtoOfGetInventoryForViewDto {
    totalCount!: number | undefined;
    items!: GetInventoryForViewDto[] | undefined;

    constructor(data?: IPagedResultDtoOfGetInventoryForViewDto) {
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
                    this.items.push(GetInventoryForViewDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfGetInventoryForViewDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfGetInventoryForViewDto();
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
}

export interface IPagedResultDtoOfGetInventoryForViewDto {
    totalCount: number | undefined;
    items: GetInventoryForViewDto[] | undefined;
}

export class GetInventoryForViewDto implements IGetInventoryForViewDto {
    inventory!: InventoryDto | undefined;
    productName!: string | undefined;
    isSold!: boolean | undefined;

    constructor(data?: IGetInventoryForViewDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.inventory = data["inventory"] ? InventoryDto.fromJS(data["inventory"]) : <any>undefined;
            this.productName = data["productName"];
            this.isSold = data["isSold"];
        }
    }

    static fromJS(data: any): GetInventoryForViewDto {
        data = typeof data === 'object' ? data : {};
        let result = new GetInventoryForViewDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["inventory"] = this.inventory ? this.inventory.toJSON() : <any>undefined;
        data["productName"] = this.productName;
        data["isSold"] = this.isSold;
        return data; 
    }
}

export interface IGetInventoryForViewDto {
    inventory: InventoryDto | undefined;
    productName: string | undefined;
    isSold: boolean | undefined;
}

export class InventoryDto implements IInventoryDto {
    tagId!: string | undefined;
    trayLevel!: number | undefined;
    price!: number | undefined;
    productId!: string | undefined;
    id!: string | undefined;

    constructor(data?: IInventoryDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tagId = data["tagId"];
            this.trayLevel = data["trayLevel"];
            this.price = data["price"];
            this.productId = data["productId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): InventoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new InventoryDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tagId"] = this.tagId;
        data["trayLevel"] = this.trayLevel;
        data["price"] = this.price;
        data["productId"] = this.productId;
        data["id"] = this.id;
        return data; 
    }
}

export interface IInventoryDto {
    tagId: string | undefined;
    trayLevel: number | undefined;
    price: number | undefined;
    productId: string | undefined;
    id: string | undefined;
}

export class GetInventoryForWebApiDto implements IGetInventoryForWebApiDto {
    inventory!: InventoryDto | undefined;
    product!: ProductDto | undefined;

    constructor(data?: IGetInventoryForWebApiDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.inventory = data["inventory"] ? InventoryDto.fromJS(data["inventory"]) : <any>undefined;
            this.product = data["product"] ? ProductDto.fromJS(data["product"]) : <any>undefined;
        }
    }

    static fromJS(data: any): GetInventoryForWebApiDto {
        data = typeof data === 'object' ? data : {};
        let result = new GetInventoryForWebApiDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["inventory"] = this.inventory ? this.inventory.toJSON() : <any>undefined;
        data["product"] = this.product ? this.product.toJSON() : <any>undefined;
        return data; 
    }
}

export interface IGetInventoryForWebApiDto {
    inventory: InventoryDto | undefined;
    product: ProductDto | undefined;
}

export class TopupDto implements ITopupDto {
    tenantId!: number | undefined;
    startDate!: moment.Moment | undefined;
    endDate!: moment.Moment | undefined;
    total!: number | undefined;
    leftOver!: number | undefined;
    sold!: number | undefined;
    error!: number | undefined;
    isProcessing!: boolean | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: string | undefined;

    constructor(data?: ITopupDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.startDate = data["startDate"] ? moment(data["startDate"].toString()) : <any>undefined;
            this.endDate = data["endDate"] ? moment(data["endDate"].toString()) : <any>undefined;
            this.total = data["total"];
            this.leftOver = data["leftOver"];
            this.sold = data["sold"];
            this.error = data["error"];
            this.isProcessing = data["isProcessing"];
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): TopupDto {
        data = typeof data === 'object' ? data : {};
        let result = new TopupDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["startDate"] = this.startDate ? this.startDate.toISOString() : <any>undefined;
        data["endDate"] = this.endDate ? this.endDate.toISOString() : <any>undefined;
        data["total"] = this.total;
        data["leftOver"] = this.leftOver;
        data["sold"] = this.sold;
        data["error"] = this.error;
        data["isProcessing"] = this.isProcessing;
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

export interface ITopupDto {
    tenantId: number | undefined;
    startDate: moment.Moment | undefined;
    endDate: moment.Moment | undefined;
    total: number | undefined;
    leftOver: number | undefined;
    sold: number | undefined;
    error: number | undefined;
    isProcessing: boolean | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: string | undefined;
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

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
import { PagedResultDtoOfMachineDto } from '@shared/service-proxies/machine-service-proxies';

@Injectable()
export class ProductMachinePriceServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(ApiServiceProxies.API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @param filter (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getProductMachinePrices(
        filter: string | null | undefined, 
        categoryFilter: string | null | undefined,
        sorting: string | null | undefined, 
        skipCount: number | null | undefined, 
        maxResultCount: number | null | undefined
    ): Observable<PagedResultDtoOfGetProductForViewDto> {
        let url_ = this.baseUrl + "/api/services/app/ProductMachinePrice/GetProductMachinePrices?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&"; 
        if (categoryFilter !== undefined)
            url_ += "CategoryFilter=" + encodeURIComponent("" + categoryFilter) + "&"; 
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
            return this.processGetProductMachinePrices(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetProductMachinePrices(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfGetProductForViewDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfGetProductForViewDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetProductMachinePrices(response: HttpResponseBase): Observable<PagedResultDtoOfGetProductForViewDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? PagedResultDtoOfGetProductForViewDto.fromJS(resultData200) : new PagedResultDtoOfGetProductForViewDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfGetProductForViewDto>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    updateProductMachinePrices(input: UpdateProductMachinePriceInput | null | undefined): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/ProductMachinePrice/UpdateProductMachinePrices";
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

        return this.http.request("put", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processUpdateProductMachinePrices(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdateProductMachinePrices(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>_observableThrow(e);
                }
            } else
                return <Observable<boolean>><any>_observableThrow(response_);
        }));
    }

    protected processUpdateProductMachinePrices(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 !== undefined ? resultData200 : <any>null;
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<boolean>(<any>null);
    }

    /**
     * @return Success
     */
    getAllMachines(skipCount: number, maxResultCount: number, sorting: string): Observable<PagedResultDtoOfMachineDto> {
        let url_ = this.baseUrl + "/api/services/app/ProductMachinePrice/GetAllMachines?";
        if (skipCount === undefined || skipCount === null)
            throw new Error("The parameter 'skipCount' must be defined and cannot be null.");
        else
            url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&";
        if (maxResultCount === undefined || maxResultCount === null)
            throw new Error("The parameter 'maxResultCount' must be defined and cannot be null.");
        else
            url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&";

        url_ += "Sorting=" + encodeURIComponent("" + sorting) + "&";

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
                    return <Observable<PagedResultDtoOfMachineDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<PagedResultDtoOfMachineDto>><any>Observable.throw(response_);
        });
    }

    protected processGetAll(response: HttpResponseBase): Observable<PagedResultDtoOfMachineDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PagedResultDtoOfMachineDto.fromJS(resultData200) : new PagedResultDtoOfMachineDto();
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
        return Observable.of<PagedResultDtoOfMachineDto>(<any>null);
    }
}

export class PagedResultDtoOfGetProductForViewDto implements IPagedResultDtoOfGetProductForViewDto {
    totalCount!: number | undefined;
    items!: ProductDto[] | undefined;

    constructor(data?: IPagedResultDtoOfGetProductForViewDto) {
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
                    this.items.push(ProductDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfGetProductForViewDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfGetProductForViewDto();
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

export interface IPagedResultDtoOfGetProductForViewDto {
    totalCount: number | undefined;
    items: ProductDto[] | undefined;
}

export class GetProductForViewDto implements IGetProductForViewDto {
    product!: ProductDto | undefined;

    constructor(data?: IGetProductForViewDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.product = data["product"] ? ProductDto.fromJS(data["product"]) : <any>undefined;
        }
    }

    static fromJS(data: any): GetProductForViewDto {
        data = typeof data === 'object' ? data : {};
        let result = new GetProductForViewDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["product"] = this.product ? this.product.toJSON() : <any>undefined;
        return data; 
    }
}

export interface IGetProductForViewDto {
    product: ProductDto | undefined;
}

export class ProductDto implements IProductDto {
    name!: string | undefined;
    sku!: string | undefined;
    barcode!: string | undefined;
    shortDesc!: string | undefined;
    desc!: string | undefined;
    tag!: string | undefined;
    imageUrl!: string | undefined;
    price!: number | undefined;
    categoriesName!: string[] | undefined;
    id!: string | undefined;

    constructor(data?: IProductDto) {
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
            this.sku = data["sku"];
            this.barcode = data["barcode"];
            this.shortDesc = data["shortDesc"];
            this.desc = data["desc"];
            this.tag = data["tag"];
            this.imageUrl = data["imageUrl"];
            this.price = data["price"];
            if (data["categoriesName"] && data["categoriesName"].constructor === Array) {
                this.categoriesName = [];
                for (let item of data["categoriesName"])
                    this.categoriesName.push(item);
            }
            this.id = data["id"];
        }
    }

    static fromJS(data: any): ProductDto {
        data = typeof data === 'object' ? data : {};
        let result = new ProductDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["sku"] = this.sku;
        data["barcode"] = this.barcode;
        data["shortDesc"] = this.shortDesc;
        data["desc"] = this.desc;
        data["tag"] = this.tag;
        data["imageUrl"] = this.imageUrl;
        data["price"] = this.price;
        if (this.categoriesName && this.categoriesName.constructor === Array) {
            data["categoriesName"] = [];
            for (let item of this.categoriesName)
                data["categoriesName"].push(item);
        }
        data["id"] = this.id;
        return data; 
    }
}

export interface IProductDto {
    name: string | undefined;
    sku: string | undefined;
    barcode: string | undefined;
    shortDesc: string | undefined;
    desc: string | undefined;
    tag: string | undefined;
    imageUrl: string | undefined;
    price: number | undefined;
    categoriesName: string[] | undefined;
    id: string | undefined;
}

export class UpdateProductMachinePriceInput implements IUpdateProductMachinePriceInput {
    machineId!: string | undefined;
    productId!: string | undefined;
    price!: number | undefined;
    sorting!: string | undefined;
    skipCount!: number | undefined;
    maxResultCount!: number | undefined;

    constructor(data?: IUpdateProductMachinePriceInput) {
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
            this.productId = data["productId"];
            this.price = data["price"];
            this.sorting = data["sorting"];
            this.skipCount = data["skipCount"];
            this.maxResultCount = data["maxResultCount"];
        }
    }

    static fromJS(data: any): UpdateProductMachinePriceInput {
        data = typeof data === 'object' ? data : {};
        let result = new UpdateProductMachinePriceInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["machineId"] = this.machineId;
        data["productId"] = this.productId;
        data["price"] = this.price;
        data["sorting"] = this.sorting;
        data["skipCount"] = this.skipCount;
        data["maxResultCount"] = this.maxResultCount;
        return data; 
    }
}

export interface IUpdateProductMachinePriceInput {
    machineId: string | undefined;
    productId: string | undefined;
    price: number | undefined;
    sorting: string | undefined;
    skipCount: number | undefined;
    maxResultCount: number | undefined;
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

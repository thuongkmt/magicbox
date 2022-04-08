
import 'rxjs/add/operator/finally';
import 'rxjs/add/observable/fromPromise';
import 'rxjs/add/observable/of';
import 'rxjs/add/observable/throw';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/toPromise';
import 'rxjs/add/operator/mergeMap';
import 'rxjs/add/operator/catch';

import { Injectable, Inject, Optional } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { blobToText, throwException } from './service-base';
import { FileDto, ProductDto, API_BASE_URL, CreateOrEditProductDto, IGetProductForEditOutput } from './service-proxies';
import { CategoryDto } from './category-service-proxies'
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';

@Injectable()
export class ProductsServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @param filter (optional) 
     * @param sKUFilter (optional) 
     * @param nameFilter (optional) 
     * @param maxPriceFilter (optional) 
     * @param minPriceFilter (optional) 
     * @param descFilter (optional) 
     * @param shortDesc1Filter (optional) 
     * @param shortDesc2Filter (optional) 
     * @param shortDesc3Filter (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getAllProducts(filter: string | null | undefined, 
        nameFilter: string | null | undefined, 
        sKUFilter: string | null | undefined, 
        barcodeFilter: string | null | undefined,
        shortDescFilter: string | null | undefined, 
        descFilter: string | null | undefined, 
        tagFilter: string | null | undefined,
        maxPriceFilter: number | null | undefined, 
        minPriceFilter: number | null | undefined, 
        categoryFilter: string | null | undefined,
        sorting: string | null | undefined, 
        skipCount: number | null | undefined, 
        maxResultCount: number | null | undefined): Observable<PagedResultDtoOfGetProductForViewDto> {
        let url_ = this.baseUrl + "/api/services/app/Products/GetAll?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&"; 

        if (sKUFilter !== undefined)
            url_ += "SKUFilter=" + encodeURIComponent("" + sKUFilter) + "&"; 

        if (nameFilter !== undefined)
            url_ += "NameFilter=" + encodeURIComponent("" + nameFilter) + "&"; 

        if (barcodeFilter !== undefined)
            url_ += "BarcodeFilter=" + encodeURIComponent("" + barcodeFilter) + "&"; 

        if (maxPriceFilter !== undefined && maxPriceFilter !== null)
            url_ += "MaxPriceFilter=" + encodeURIComponent("" + maxPriceFilter) + "&"; 

        if (minPriceFilter !== undefined && minPriceFilter !== null)
            url_ += "MinPriceFilter=" + encodeURIComponent("" + minPriceFilter) + "&"; 

        if (descFilter !== undefined)
            url_ += "DescFilter=" + encodeURIComponent("" + descFilter) + "&"; 

        if (shortDescFilter !== undefined)
            url_ += "ShortDescFilter=" + encodeURIComponent("" + shortDescFilter) + "&"; 
        
        if (tagFilter !== undefined)
        url_ += "TagFilter=" + encodeURIComponent("" + tagFilter) + "&"; 
        
        if (categoryFilter !== undefined && categoryFilter !== null)
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
            return this.processGetAllProducts(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllProducts(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfGetProductForViewDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfGetProductForViewDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetAllProducts(response: HttpResponseBase): Observable<PagedResultDtoOfGetProductForViewDto> {
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

    getAllProductsNoFilter(): Observable<PagedResultDtoOfGetProductForViewDto> {
        let url_ = this.baseUrl + "/api/services/app/Products/GetAllNoFilter?";
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetAllProductsNoFilter(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllProducts(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfGetProductForViewDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfGetProductForViewDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetAllProductsNoFilter(response: HttpResponseBase): Observable<PagedResultDtoOfGetProductForViewDto> {
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
     * @param id (optional) 
     * @return Success
     */
    getProductForView(id: string | null | undefined): Observable<ProductDto> {
        let url_ = this.baseUrl + "/api/services/app/Products/GetProductForView?";
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
            return this.processGetProductForView(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetProductForView(<any>response_);
                } catch (e) {
                    return <Observable<ProductDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<ProductDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetProductForView(response: HttpResponseBase): Observable<ProductDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? ProductDto.fromJS(resultData200) : new ProductDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<ProductDto>(<any>null);
    }

    /**
     * @param id (optional) 
     * @return Success
     */
    getProductForEdit(id: string | null | undefined): Observable<GetProductForEditOutput> {
        let url_ = this.baseUrl + "/api/services/app/Products/GetProductForEdit?";
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
            return this.processGetProductForEdit(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetProductForEdit(<any>response_);
                } catch (e) {
                    return <Observable<GetProductForEditOutput>><any>_observableThrow(e);
                }
            } else
                return <Observable<GetProductForEditOutput>><any>_observableThrow(response_);
        }));
    }

    protected processGetProductForEdit(response: HttpResponseBase): Observable<GetProductForEditOutput> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? GetProductForEditOutput.fromJS(resultData200) : new GetProductForEditOutput();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<GetProductForEditOutput>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    createOrEdit(input: CreateOrEditProductDto | null | undefined): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/Products/CreateOrEdit";
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
     * @param id (optional) 
     * @return Success
     */
    delete(id: string | null | undefined): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/Products/Delete?";
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
     * @param sKUFilter (optional) 
     * @param nameFilter (optional) 
     * @param maxPriceFilter (optional) 
     * @param minPriceFilter (optional) 
     * @param descFilter (optional) 
     * @param shortDesc1Filter (optional) 
     * @param shortDesc2Filter (optional) 
     * @param shortDesc3Filter (optional) 
     * @param imageUrlFilter (optional) 
     * @return Success
     */
    getProductsToExcel(
        filter: string | null | undefined, 
        nameFilter: string | null | undefined, 
        sKUFilter: string | null | undefined, 
        barcodeFilter: string | null | undefined,
        shortDescFilter: string | null | undefined, 
        descFilter: string | null | undefined, 
        tagFilter: string | null | undefined,
        //imageUrlFilter: string | null | undefined,
        maxPriceFilter: number | null | undefined, 
        minPriceFilter: number | null | undefined
    ): Observable<FileDto> {
        let url_ = this.baseUrl + "/api/services/app/Products/GetProductsToExcel?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&"; 

        if (sKUFilter !== undefined)
            url_ += "SKUFilter=" + encodeURIComponent("" + sKUFilter) + "&"; 

        if (nameFilter !== undefined)
            url_ += "NameFilter=" + encodeURIComponent("" + nameFilter) + "&"; 
        
        if (descFilter !== undefined)
            url_ += "DescFilter=" + encodeURIComponent("" + descFilter) + "&"; 

        if (shortDescFilter !== undefined)
            url_ += "ShortDescFilter=" + encodeURIComponent("" + shortDescFilter) + "&"; 

        if (tagFilter !== undefined)
            url_ += "TagFilter=" + encodeURIComponent("" + tagFilter) + "&"; 

        if (maxPriceFilter !== undefined && maxPriceFilter !== null)
            url_ += "MaxPriceFilter=" + encodeURIComponent("" + maxPriceFilter) + "&"; 

        if (minPriceFilter !== undefined && minPriceFilter != null)
            url_ += "MinPriceFilter=" + encodeURIComponent("" + minPriceFilter) + "&"; 

        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetProductsToExcel(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetProductsToExcel(<any>response_);
                } catch (e) {
                    return <Observable<FileDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<FileDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetProductsToExcel(response: HttpResponseBase): Observable<FileDto> {
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

    import(items: CreateOrEditProductDto[] | null | undefined): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/Products/Import";
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
            return this.processImport(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processImport(<any>response_);
                } catch (e) {
                    return <Observable<void>><any>_observableThrow(e);
                }
            } else
                return <Observable<void>><any>_observableThrow(response_);
        }));
    }

    protected processImport(response: HttpResponseBase): Observable<void> {
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

export class GetProductForEditOutput implements IGetProductForEditOutput {
    product!: CreateOrEditProductDto | undefined;

    constructor(data?: IGetProductForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.product = data["product"] ? CreateOrEditProductDto.fromJS(data["product"]) : <any>undefined;
        }
    }

    static fromJS(data: any): GetProductForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetProductForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["product"] = this.product ? this.product.toJSON() : <any>undefined;
        return data; 
    }
}

// export interface IGetProductForEditOutput {
//     product: CreateOrEditProductDto | undefined;
// }

// export class CreateOrEditProductDto implements ICreateOrEditProductDto {
//     name!: string;
//     sku!: string;
//     barcode!: string;
//     shortDesc!: string | undefined;
//     desc!: string | undefined;
//     tag!: string | undefined;
//     imageUrl!: string | undefined;
//     categoryIds!: string[] | undefined;
//     categoryNames!: string | undefined;
//     price!: number | undefined;
//     id!: string | undefined;

//     constructor(data?: ICreateOrEditProductDto) {
//         if (data) {
//             for (var property in data) {
//                 if (data.hasOwnProperty(property))
//                     (<any>this)[property] = (<any>data)[property];
//             }
//         }
//     }

//     init(data?: any) {
//         if (data) {
//             this.name = data["name"];
//             this.sku = data["sku"];
//             this.barcode = data["barcode"];
//             this.shortDesc = data["shortDesc"];
//             this.desc = data["desc"];
//             this.tag = data["tag"];
//             this.imageUrl = data["imageUrl"];
//             this.categoryIds = data["categoryIds"];
//             this.categoryNames = data["categoryNames"]; 
//             this.price = data["price"];
//             this.id = data["id"];
//         }
//     }

//     static fromJS(data: any): CreateOrEditProductDto {
//         data = typeof data === 'object' ? data : {};
//         let result = new CreateOrEditProductDto();
//         result.init(data);
//         return result;
//     }

//     toJSON(data?: any) {
//         data = typeof data === 'object' ? data : {};
//         data["name"] = this.name;
//         data["sku"] = this.sku;
//         data["barcode"] = this.barcode;
//         data["shortDesc"] = this.shortDesc;
//         data["desc"] = this.desc;
//         data["tag"] = this.tag;
//         data["imageUrl"] = this.imageUrl;
//         data["categoryIds"] = this.categoryIds;
//         data["categoryNames"] = this.categoryNames;
//         data["price"] = this.price;
//         data["id"] = this.id;
//         return data; 
//     }
// }

// export interface ICreateOrEditProductDto {
//     name: string;
//     sku: string;
//     barcode: string;
//     shortDesc: string | undefined;
//     desc: string | undefined;
//     tag: string | undefined;
//     imageUrl: string | undefined;
//     categoryIds: string[] | undefined;
//     price: number | undefined;
//     id: string | undefined;
// }
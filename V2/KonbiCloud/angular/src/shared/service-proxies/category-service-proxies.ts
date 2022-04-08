
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
import {  ListResultDtoOfPermissionDto, API_BASE_URL, FileDto, } from './service-proxies';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';


//category


@Injectable()
export class CategoryServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor( @Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @param filter (optional) 
     * @param nameFilter (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getAll(filter: string | null | undefined, 
        nameFilter: string | null | undefined, 
        codeFilter: string | null | undefined,
        descFilter: string | null | undefined,
        sorting: string | null | undefined, 
        skipCount: number | null | undefined, 
        maxResultCount: number | null | undefined
    ): Observable<PagedResultDtoOfGetProductCategoryForViewDto> {
        let url_ = this.baseUrl + "/api/services/app/ProductCategories/GetAll?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&";

        if (nameFilter !== undefined)
            url_ += "NameFilter=" + encodeURIComponent("" + nameFilter) + "&"; 

        if (codeFilter !== undefined)
            url_ += "CodeFilter=" + encodeURIComponent("" + codeFilter) + "&"; 

        if (nameFilter !== undefined)
            url_ += "DescFilter=" + encodeURIComponent("" + descFilter) + "&"; 

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
                    return <Observable<PagedResultDtoOfGetProductCategoryForViewDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfGetProductCategoryForViewDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetAll(response: HttpResponseBase): Observable<PagedResultDtoOfGetProductCategoryForViewDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? PagedResultDtoOfGetProductCategoryForViewDto.fromJS(resultData200) : new PagedResultDtoOfGetProductCategoryForViewDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfGetProductCategoryForViewDto>(<any>null);
    }

    /**
     * @param id (optional) 
     * @return Success
     */
    getProductCategoryForView(id: string | null | undefined): Observable<CategoryDto> {
        let url_ = this.baseUrl + "/api/services/app/ProductCategories/GetProductCategoryForView?";
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
            return this.processGetProductCategoryForView(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetProductCategoryForView(<any>response_);
                } catch (e) {
                    return <Observable<CategoryDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<CategoryDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetProductCategoryForView(response: HttpResponseBase): Observable<CategoryDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? CategoryDto.fromJS(resultData200) : new CategoryDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<CategoryDto>(<any>null);
    }

    /**
     * @param id (optional) 
     * @return Success
     */
    getProductCategoryForEdit(id: string | null | undefined): Observable<GetProductCategoryForEditOutput> {
        let url_ = this.baseUrl + "/api/services/app/ProductCategories/GetProductCategoryForEdit?";
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
            return this.processGetProductCategoryForEdit(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetProductCategoryForEdit(<any>response_);
                } catch (e) {
                    return <Observable<GetProductCategoryForEditOutput>><any>_observableThrow(e);
                }
            } else
                return <Observable<GetProductCategoryForEditOutput>><any>_observableThrow(response_);
        }));
    }

    protected processGetProductCategoryForEdit(response: HttpResponseBase): Observable<GetProductCategoryForEditOutput> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? GetProductCategoryForEditOutput.fromJS(resultData200) : new GetProductCategoryForEditOutput();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<GetProductCategoryForEditOutput>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    createOrEdit(input: CreateOrEditProductCategoryDto | null | undefined): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/ProductCategories/CreateOrEdit";
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
        let url_ = this.baseUrl + "/api/services/app/ProductCategories/Delete?";
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
     * @param nameFilter (optional) 
     * @return Success
     */
    getProductCategoriesToExcel(
        filter: string | null | undefined, 
        nameFilter: string | null | undefined,
        codeFilter: string | null | undefined,
        descFilter: string | null | undefined
    ): Observable<FileDto> {
        let url_ = this.baseUrl + "/api/services/app/ProductCategories/GetProductCategoriesToExcel?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&"; 
        
        if (nameFilter !== undefined)
            url_ += "NameFilter=" + encodeURIComponent("" + nameFilter) + "&";
            
        if (codeFilter !== undefined)
            url_ += "CodeFilter=" + encodeURIComponent("" + codeFilter) + "&";

        if (nameFilter !== undefined)
            url_ += "DescFilter=" + encodeURIComponent("" + descFilter) + "&";

        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetProductCategoriesToExcel(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetProductCategoriesToExcel(<any>response_);
                } catch (e) {
                    return <Observable<FileDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<FileDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetProductCategoriesToExcel(response: HttpResponseBase): Observable<FileDto> {
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
}
export class PagedResultDtoOfGetProductCategoryForViewDto implements IPagedResultDtoOfGetProductCategoryForViewDto {
    totalCount!: number | undefined;
    items!: CategoryDto[] | undefined;

    constructor(data?: IPagedResultDtoOfGetProductCategoryForViewDto) {
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
                    this.items.push(CategoryDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfGetProductCategoryForViewDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfGetProductCategoryForViewDto();
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

export interface IPagedResultDtoOfGetProductCategoryForViewDto {
    totalCount: number | undefined;
    items: CategoryDto[] | undefined;
}

export class ProductCategoryDto implements IProductCategoryDto {
    name!: string | undefined;
    code!: string | undefined;
    desc!: string | undefined;
    creationTime: Date | undefined;
    id!: string | undefined;

    constructor(data?: IProductCategoryDto) {
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
            this.code = data["code"];
            this.desc = data["desc"];
            this.creationTime = data["creationTime"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): ProductCategoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new ProductCategoryDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["code"] = this.code;
        data["desc"] = this.desc;
        data["creationTime"] = this.creationTime;
        data["id"] = this.id;
        return data; 
    }
}

export interface IProductCategoryDto {
    name: string | undefined;
    code: string | undefined;
    desc: string | undefined;
    creationTime: Date | undefined;
    id: string | undefined;
}

export class GetProductCategoryForEditOutput implements IGetProductCategoryForEditOutput {
    productCategory!: CreateOrEditProductCategoryDto | undefined;

    constructor(data?: IGetProductCategoryForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.productCategory = data["productCategory"] ? CreateOrEditProductCategoryDto.fromJS(data["productCategory"]) : <any>undefined;
        }
    }

    static fromJS(data: any): GetProductCategoryForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetProductCategoryForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["productCategory"] = this.productCategory ? this.productCategory.toJSON() : <any>undefined;
        return data; 
    }
}

export interface IGetProductCategoryForEditOutput {
    productCategory: CreateOrEditProductCategoryDto | undefined;
}

export class CreateOrEditProductCategoryDto implements ICreateOrEditProductCategoryDto {
    name!: string;
    code!: string | undefined;
    desc!: string | undefined;
    id!: string | undefined;

    constructor(data?: ICreateOrEditProductCategoryDto) {
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
            this.code = data["code"];
            this.desc = data["desc"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CreateOrEditProductCategoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrEditProductCategoryDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["code"] = this.code;
        data["desc"] = this.desc;
        data["id"] = this.id;
        return data; 
    }
}

export interface ICreateOrEditProductCategoryDto {
    name: string;
    code: string | undefined;
    desc: string | undefined;
    id: string | undefined;
}

export class SelectionCategory {
    name: string;
    id: string;
    checked: boolean;
  
    constructor(id: string, name: string, checked: boolean) {
      this.id = id;
      this.name = name;
      this.checked = checked;
    }
}

export class CategoryDto implements ICategoryDto {
    id: string;
    name: string;
    code: string;
    desc :string;
    creationTime: Date;

    constructor(data?: ICategoryDto) {
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
            this.desc = data["desc"];
            this.creationTime = data["creationTime"];
        }
    }

    static fromJS(data: any): CategoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new CategoryDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["name"] = this.name;
        data["code"] = this.code;
        data["desc"] = this.desc;
        data["creationTime"] = this.creationTime;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new CategoryDto();
        result.init(json);
        return result;
    }
}

export interface ICategoryDto {
    name: string;
}
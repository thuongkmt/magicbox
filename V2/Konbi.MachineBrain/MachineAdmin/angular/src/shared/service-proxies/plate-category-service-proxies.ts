// import 'rxjs/add/operator/finally';
// import 'rxjs/add/observable/fromPromise';
// import 'rxjs/add/observable/of';
// import 'rxjs/add/observable/throw';
// import 'rxjs/add/operator/map';
// import 'rxjs/add/operator/toPromise';
// import 'rxjs/add/operator/mergeMap';
// import 'rxjs/add/operator/catch';
// import { Observable } from 'rxjs/Observable';
// import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
// import { HttpClient, HttpHeaders, HttpParams, HttpResponse, HttpResponseBase, HttpErrorResponse } from '@angular/common/http';

import * as moment from 'moment';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
//export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');
import {  ListResultDtoOfPermissionDto, API_BASE_URL } from './service-proxies';
import { blobToText, throwException } from './service-base';
@Injectable()
export class PlateCategoryServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    createOrUpdate(input: CreateOrEditPlateCategoryDto | null | undefined): Observable<CreateOrEditPlateCategoryDto> {
        let url_ = this.baseUrl + "/api/services/app/PlateCategories/CreateOrEdit";
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
            return this.processCreateOrUpdate(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processCreateOrUpdate(<any>response_);
                } catch (e) {
                    return <Observable<CreateOrEditPlateCategoryDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<CreateOrEditPlateCategoryDto>><any>Observable.throw(response_);
        });
    }

    protected processCreateOrUpdate(response: HttpResponseBase): Observable<CreateOrEditPlateCategoryDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? CreateOrEditPlateCategoryDto.fromJS(resultData200) : new CreateOrEditPlateCategoryDto();
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
        return Observable.of<CreateOrEditPlateCategoryDto>(<any>null);
    }

    /**
     * @return Success
     */
    delete(id: number): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/PlateCategories/Delete?";
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
            return this.processDelete(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processDelete(<any>response_);
                } catch (e) {
                    return <Observable<void>><any>Observable.throw(e);
                }
            } else
                return <Observable<void>><any>Observable.throw(response_);
        });
    }

    protected processDelete(response: HttpResponseBase): Observable<void> {
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

    syncData(): Observable<any> {
        let url_ = this.baseUrl + "/api/services/app/PlateCategories/SyncPlateCategoryData";
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

    /**
     * @return Success
     */
    getAllPermissions(): Observable<ListResultDtoOfPermissionDto> {
        let url_ = this.baseUrl + "/api/services/app/Role/GetAllPermissions";
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
            return this.processGetAllPermissions(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllPermissions(<any>response_);
                } catch (e) {
                    return <Observable<ListResultDtoOfPermissionDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<ListResultDtoOfPermissionDto>><any>Observable.throw(response_);
        });
    }

    protected processGetAllPermissions(response: HttpResponseBase): Observable<ListResultDtoOfPermissionDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? ListResultDtoOfPermissionDto.fromJS(resultData200) : new ListResultDtoOfPermissionDto();
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
        return Observable.of<ListResultDtoOfPermissionDto>(<any>null);
    }

    /**
     * @return Success
     */
    get(id: number): Observable<GetPlateCategoryForEditOutput> {
        let url_ = this.baseUrl + "/api/services/app/PlateCategories/GetPlateCategoryForEdit?";

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
            return this.processGet(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGet(<any>response_);
                } catch (e) {
                    return <Observable<GetPlateCategoryForEditOutput>><any>Observable.throw(e);
                }
            } else
                return <Observable<GetPlateCategoryForEditOutput>><any>Observable.throw(response_);
        });
    }

    protected processGet(response: HttpResponseBase): Observable<GetPlateCategoryForEditOutput> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? GetPlateCategoryForEditOutput.fromJS(resultData200) : new GetPlateCategoryForEditOutput();
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
        return Observable.of<GetPlateCategoryForEditOutput>(<any>null);
    }

    /**
     * @return Success
     */
    getAll(
        filter: string | null | undefined,
        nameFilter: string | null | undefined,
        sorting: string | null | undefined,
        maxResultCount: number | null | undefined,
        skipCount: number | null | undefined
    ): Observable<PagedResultDtoOfPlateCategoryDto> {
        let url_ = this.baseUrl + "/api/services/app/PlateCategories/GetAll?";


        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&";
        if (nameFilter !== undefined)
            url_ += "NameFilter=" + encodeURIComponent("" + nameFilter) + "&";
        if (sorting !== undefined)
            url_ += "Sorting=" + encodeURIComponent("" + sorting) + "&";
        if (maxResultCount !== undefined)
            url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&";
        if (skipCount !== undefined)
            url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetAll(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAll(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfPlateCategoryDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfPlateCategoryDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetAll(response: HttpResponseBase): Observable<PagedResultDtoOfPlateCategoryDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PagedResultDtoOfPlateCategoryDto.fromJS(resultData200) : new PagedResultDtoOfPlateCategoryDto();
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
        return Observable.of<PagedResultDtoOfPlateCategoryDto>(<any>null);
    }

}

export class CreatePlateCategoryDto implements ICreatePlateCategoryDto {
    name: string;
    desc: string;
    // imageUrl: string;
    // fileContent: string;
    constructor(data?: ICreatePlateCategoryDto) {
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
            this.desc = data["desc"];
            // this.fileContent = data["fileContent"];
            // this.imageUrl = data["imageUrl"];
        }
    }

    static fromJS(data: any): CreatePlateCategoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreatePlateCategoryDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["desc"] = this.desc;
        // data["fileContent"] = this.fileContent;
        // data["imageUrl"] = this.imageUrl;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new CreatePlateCategoryDto();
        result.init(json);
        return result;
    }
}

export interface ICreatePlateCategoryDto {
    name: string;
    desc: string;
    // imageUrl: string;
    // fileContent: string;
}

/**********  VIEW PLATE CATEGORY DTO **********/

export class GetPlateCategoryForView implements IGetPlateCategoryForView {
    plateCategory: PlateCategoryDto;

    constructor(data?: IGetPlateCategoryForView) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.plateCategory = data["plateCategory"];
        }
    }

    static fromJS(data: any): GetPlateCategoryForView {
        data = typeof data === 'object' ? data : {};
        let result = new GetPlateCategoryForView();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["plateCategory"] = this.plateCategory;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new GetPlateCategoryForView();
        result.init(json);
        return result;
    }
}

export interface IGetPlateCategoryForView {
    plateCategory: PlateCategoryDto;
}

/********** END VIEW PLATE CATEGORY DTO **********/


/**********  CREATE OR EDIT PLATE CATEGORY DTO **********/

export class GetPlateCategoryForEditOutput implements IGetPlateCategoryForEditOutput {
    plateCategory: CreateOrEditPlateCategoryDto;

    constructor(data?: IGetPlateCategoryForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.plateCategory = data["plateCategory"];
        }
    }

    static fromJS(data: any): GetPlateCategoryForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetPlateCategoryForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["plateCategory"] = this.plateCategory;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new GetPlateCategoryForEditOutput();
        result.init(json);
        return result;
    }
}

export interface IGetPlateCategoryForEditOutput {
    plateCategory: CreateOrEditPlateCategoryDto;
}

export class CreateOrEditPlateCategoryDto implements ICreateOrEditPlateCategoryDto {

    name: string;
    desc: string;
    id!: number | undefined;

    constructor(data?: ICreateOrEditPlateCategoryDto) {
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
            this.desc = data["desc"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CreateOrEditPlateCategoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrEditPlateCategoryDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["desc"] = this.desc;
        data["id"] = this.id;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new CreateOrEditPlateCategoryDto();
        result.init(json);
        return result;
    }
}

export interface ICreateOrEditPlateCategoryDto {
    name: string;
    desc: string;
    id: number | undefined;
}



/********** END CREATE OR EDIT PLATE CATEGORY DTO **********/

export class PlateCategoryDto implements IPlateCategoryDto {
    name: string;
    desc: string;
    id!: number | undefined;
    plates: any;

    constructor(data?: IPlateCategoryDto) {
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
            this.desc = data["desc"];
            this.id = data["id"];
            this.plates = data["plates"];
        }
    }

    static fromJS(data: any): PlateCategoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new PlateCategoryDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["desc"] = this.desc;
        data["id"] = this.id;
        data["plates"] = this.plates;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new PlateCategoryDto();
        result.init(json);
        return result;
    }
}

export interface IPlateCategoryDto {
    name: string;
    desc: string;
    id: number | undefined;
    plates: any;
}

export class PagedResultDtoOfPlateCategoryDto implements IPagedResultDtoOfPlateCategoryDto {
    totalCount: number | undefined;
    items: GetPlateCategoryForView[] | undefined;

    constructor(data?: IPagedResultDtoOfPlateCategoryDto) {
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
                    this.items.push(GetPlateCategoryForView.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfPlateCategoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfPlateCategoryDto();
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
        let result = new PagedResultDtoOfPlateCategoryDto();
        result.init(json);
        return result;
    }
}

export interface IPagedResultDtoOfPlateCategoryDto {
    totalCount: number | undefined;
    items: GetPlateCategoryForView[] | undefined;
}
// end plateCategory


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
// import * as moment from 'moment';
import {  ListResultDtoOfPermissionDto, API_BASE_URL } from './service-proxies';
import { PlateCategoryDto } from './plate-category-service-proxies'
import { blobToText, throwException } from './service-base';

import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';

import * as moment from 'moment';


@Injectable()
export class PlateServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @input (optional) 
     * @return Success
     */
    createOrEdit(input: CreateOrEditPlateDto | null | undefined): Observable<PlateMessage> {
        let url_ = this.baseUrl + "/api/services/app/Plates/CreateOrEdit";
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
            return this.processCreate(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processCreate(<any>response_);
                } catch (e) {
                    return <Observable<PlateMessage>><any>Observable.throw(e);
                }
            } else
                return <Observable<PlateMessage>><any>Observable.throw(response_);
        });
    }

    protected processCreate(response: HttpResponseBase): Observable<PlateMessage> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PlateMessage.fromJS(resultData200) : new PlateMessage();
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
        return Observable.of<PlateMessage>(<any>null);
    }

    /**
     * @return Success
     */
    delete(id: string): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/Plates/Delete?";
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

    /**
     * @return Success
     */
    get(id: string): Observable<GetPlateForEditOutput> {
        let url_ = this.baseUrl + "/api/services/app/Plates/GetPlateForEdit?";
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
            console.log(response_)
            return this.processGet(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGet(<any>response_);
                } catch (e) {
                    return <Observable<GetPlateForEditOutput>><any>Observable.throw(e);
                }
            } else
                return <Observable<GetPlateForEditOutput>><any>Observable.throw(response_);
        });
    }

    protected processGet(response: HttpResponseBase): Observable<GetPlateForEditOutput> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? GetPlateForEditOutput.fromJS(resultData200) : new GetPlateForEditOutput();
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
        return Observable.of<GetPlateForEditOutput>(<any>null);
    }

    /**
     * @return Success
     */

    getAll(
        filter: string | null | undefined,
        nameFilter: string | null | undefined,
        //imageUrlFilter: string | null | undefined,
        //descFilter: string | null | undefined,
        codeFilter: string | null | undefined,
        // maxAvaiableFilter: number | null | undefined,
        // minAvaiableFilter: number | null | undefined,
        colorFilter: string | null | undefined,
        plateCategoryNameFilter: string | null | undefined,
        sorting: string | null | undefined,
        maxResultCount: number | null | undefined,
        skipCount: number | null | undefined
    ): Observable<PagedResultDtoOfPlateDto> {

        let url_ = this.baseUrl + "/api/services/app/Plates/GetAll?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&";
        if (nameFilter !== undefined)
            url_ += "NameFilter=" + encodeURIComponent("" + nameFilter) + "&";
        if (codeFilter !== undefined)
            url_ += "CodeFilter=" + encodeURIComponent("" + codeFilter) + "&";
        // if (maxAvaiableFilter !== undefined)
        //     url_ += "MaxAvaiableFilter=" + encodeURIComponent("" + maxAvaiableFilter) + "&";
        // if (minAvaiableFilter !== undefined)
        //     url_ += "MinAvaiableFilter=" + encodeURIComponent("" + minAvaiableFilter) + "&";
        if (plateCategoryNameFilter !== undefined)
            url_ += "PlateCategoryNameFilter=" + encodeURIComponent("" + plateCategoryNameFilter) + "&";
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
                    return <Observable<PagedResultDtoOfPlateDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfPlateDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetAll(response: HttpResponseBase): Observable<PagedResultDtoOfPlateDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PagedResultDtoOfPlateDto.fromJS(resultData200) : new PagedResultDtoOfPlateDto();
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
        return Observable.of<PagedResultDtoOfPlateDto>(<any>null);
    }

    importPlate(input: CreateOrEditPlateDto[]): Observable<ImportResult> {
        let url_ = this.baseUrl + "/api/services/app/Plates/ImportPlate";
        url_ = url_.replace(/[?&]$/, "");
        // var content_ = {};
        // input.toJSON(content_);
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
            return this.processImportPlate(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processImportPlate(<any>response_);
                } catch (e) {
                    return <Observable<ImportResult>><any>Observable.throw(e);
                }
            } else
                return <Observable<ImportResult>><any>Observable.throw(response_);
        });
    }

    protected processImportPlate(response: HttpResponseBase): Observable<ImportResult> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? ImportResult.fromJS(resultData200) : new ImportResult();
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
        return Observable.of<ImportResult>(<any>null);
    }


    syncPlate(): Observable<PlateMessage> {
        let url_ = this.baseUrl + "/api/services/app/Plates/SyncPlateData";
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
                    return <Observable<PlateMessage>><any>Observable.throw(e);
                }
            } else
                return <Observable<PlateMessage>><any>Observable.throw(response_);
        });
    }

    protected processSyncData(response: HttpResponseBase): Observable<PlateMessage> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PlateMessage.fromJS(resultData200) : new PlateMessage();
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
        return Observable.of<PlateMessage>(<any>null);
    }


    uploadFile(files: Array<File>): Observable<any> {
        var url_ = this.baseUrl + '/PlateImageUpload/UploadFiles'

        const formData: any = new FormData()
        console.log(files)

        for (let i = 0; i < files.length; i++) {
            formData.append("files", files[i], files[i]['name']);
        }
        console.log('form data variable :   ' + formData.toString());

        return this.http
            .post(url_, formData)
            .map(res => { return res })
            .catch(error => {
                let errMsg = (error.message) ? error.message :
                    error.status ? `${error.status} - ${error.statusText}` : 'Server error';
                console.error(errMsg);
                return Observable.throw(errMsg);
            });
    }

    importCsvUploadImages(files: Array<File>): Observable<any> {
        var url_ = this.baseUrl + '/PlateImageUpload/ImportPlateUploadFiles'

        const formData: any = new FormData()
        console.log(files)

        for (let i = 0; i < files.length; i++) {
            formData.append("files", files[i], files[i]['name']);
        }
        console.log('form data variable :   ' + formData.toString());

        return this.http
            .post(url_, formData)
            .map(res => { return res })
            .catch(error => {
                let errMsg = (error.message) ? error.message :
                    error.status ? `${error.status} - ${error.statusText}` : 'Server error';
                console.error(errMsg);
                return Observable.throw(errMsg);
            });
    }

}

//******** CREATE OR EDIT PLATE DTO ********/
export class GetPlateForEditOutput implements IGetPlateForEditOutput {

    plate: CreateOrEditPlateDto;
    plateCategoryName: string;

    constructor(data?: IGetPlateForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.plate = data["plate"];
            this.plateCategoryName = data["plateCategoryName"];
        }
    }

    static fromJS(data: any): GetPlateForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetPlateForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["plate"] = this.plate;
        data["plateCategoryName"] = this.plateCategoryName;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new GetPlateForEditOutput();
        result.init(json);
        return result;
    }
}

export class PlateMessage {
    message: string;

    init(data?: any) {
        if (data) {
            this.message = data["message"];
        }
    }

    static fromJS(data: any): PlateMessage {
        data = typeof data === 'object' ? data : {};
        let result = new PlateMessage();
        result.init(data);
        return result;
    }
}

export interface IGetPlateForEditOutput {
    plate: CreateOrEditPlateDto;
    plateCategoryName: string;
}

export class CreateOrEditPlateDto implements ICreateOrEditPlateDto {
    name: string;
    imageUrl: string;
    desc: string;
    code: string;
    avaiable: number;
    color: string;
    plateCategoryId: number;
    id!: string | undefined;

    constructor(data?: ICreateOrEditPlateDto) {
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
            this.imageUrl = data["imageUrl"];
            this.desc = data["desc"];
            this.code = data["code"];
            this.avaiable = data["avaiable"];
            this.color = data["color"];
            this.plateCategoryId = data["plateCategoryId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CreateOrEditPlateDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrEditPlateDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["imageUrl"] = this.imageUrl;
        data["desc"] = this.desc;
        data["code"] = this.code;
        data["avaiable"] = this.avaiable;
        data["color"] = this.color;
        data["plateCategoryId"] = this.plateCategoryId;
        data["id"] = this.id;

        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new CreateOrEditPlateDto();
        result.init(json);
        return result;
    }
}

export interface ICreateOrEditPlateDto {
    name: string;
    imageUrl: string;
    desc: string;
    code: string;
    avaiable: number;
    color: string;
    plateCategoryId: number;
    id: string;
}
//******** END CREATE OR EDIT PLATE DTO ********/

//******** PRODUCT DTO ********/

export class GetPlateForView implements IGetPlateForView {
    plateCategoryName: string;
    plate: PlateDto;

    constructor(data?: IGetPlateForView) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.plateCategoryName = data["plateCategoryName"];
            this.plate = data["plate"];
        }
    }

    static fromJS(data: any): GetPlateForView {
        data = typeof data === 'object' ? data : {};
        let result = new GetPlateForView();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["plateCategoryName"] = this.plateCategoryName;
        data["plate"] = this.plate;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new GetPlateForView();
        result.init(json);
        return result;
    }
}

export interface IGetPlateForView {
    plateCategoryName: string;
    plate: PlateDto;
}


export class PlateDto implements IPlateDto {

    id: string | undefined;
    name: string;
    imageUrl: string;
    desc: string;
    code: string;
    avaiable: number;
    color: string;
    plateCategoryId: number;

    discs: any;

    constructor(data?: IPlateDto) {
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
            this.imageUrl = data["imageUrl"];
            this.desc = data["desc"];
            this.code = data["code"];
            this.avaiable = data["avaiable"];
            this.plateCategoryId = data["plateCategoryId"];
            this.discs = data["discs"];
        }
    }

    static fromJS(data: any): PlateDto {
        data = typeof data === 'object' ? data : {};
        let result = new PlateDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["name"] = this.name;
        data["imageUrl"] = this.imageUrl;
        data["desc"] = this.desc;
        data["code"] = this.code;
        data["avaiable"] = this.avaiable;
        data["color"] = this.color;
        data["plateCategoryId"] = this.plateCategoryId;

        data["discs"] = this.discs;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new PlateDto();
        result.init(json);
        return result;
    }
}

export interface IPlateDto {

    id: string;
    name: string;
    imageUrl: string;
    desc: string;
    code: string;
    avaiable: number;
    color: string;
    plateCategoryId: number;

    discs: any;
}
//******** END PRODUCT DTO ********/

export class PagedResultDtoOfPlateDto implements IPagedResultDtoOfPlateDto {
    totalCount: number | undefined;
    items: GetPlateForView[] | undefined;

    constructor(data?: IPagedResultDtoOfPlateDto) {
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
                    this.items.push(GetPlateForView.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfPlateDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfPlateDto();
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
        let result = new PagedResultDtoOfPlateDto();
        result.init(json);
        return result;
    }
}

export interface IPagedResultDtoOfPlateDto {
    totalCount: number | undefined;
    items: GetPlateForView[] | undefined;
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

export class ImportPlateCsvDto {
    lstPlate: CreateOrEditPlateDto[];
}



export class ImportResult {
    errorList: string;
    errorCount: number;
    successCount: number;

    constructor(data?: ImportResult) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.errorList = data["errorList"];
            this.errorCount = data["errorCount"];
            this.successCount = data["successCount"];
        }
    }

    static fromJS(data: any): ImportResult {
        data = typeof data === 'object' ? data : {};
        let result = new ImportResult();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["errorList"] = this.errorList;
        data["errorCount"] = this.errorCount;
        data["successCount"] = this.successCount;
    }

    clone() {
        const json = this.toJSON();
        let result = new ImportResult();
        result.init(json);
        return result;
    }
}

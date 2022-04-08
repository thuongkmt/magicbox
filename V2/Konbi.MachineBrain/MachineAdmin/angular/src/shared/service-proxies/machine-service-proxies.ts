
import 'rxjs/add/operator/finally';
import 'rxjs/add/observable/fromPromise';
import 'rxjs/add/observable/of';
import 'rxjs/add/observable/throw';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/toPromise';
import 'rxjs/add/operator/mergeMap';
import 'rxjs/add/operator/catch';

import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';

// import { Observable } from 'rxjs/Observable';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams, HttpResponse, HttpResponseBase, HttpErrorResponse } from '@angular/common/http';

import * as moment from 'moment';

//export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

import { blobToText, throwException } from './service-base';
import {  ListResultDtoOfPermissionDto, API_BASE_URL } from './service-proxies';

//Machine
@Injectable()
export class MachineServiceProxy {
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
    create(input: CreateMachineDto | null | undefined): Observable<MachineDto> {
        let url_ = this.baseUrl + "/api/services/app/Machine/Create";
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
                    return <Observable<MachineDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<MachineDto>><any>Observable.throw(response_);
        });
    }

    protected processCreate(response: HttpResponseBase): Observable<MachineDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? MachineDto.fromJS(resultData200) : new MachineDto();
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
        return Observable.of<MachineDto>(<any>null);
    }

    /**
     * @input (optional) 
     * @return Success
     */
    update(input: MachineDto | null | undefined): Observable<MachineDto> {
        let url_ = this.baseUrl + "/api/services/app/Machine/Update";
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
                    return <Observable<MachineDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<MachineDto>><any>Observable.throw(response_);
        });
    }

    protected processUpdate(response: HttpResponseBase): Observable<MachineDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? resultData200 : new MachineDto();
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
        return Observable.of<MachineDto>(<any>null);
    }

    /**
     * process update inventory item
     * **/
    updateUpdateMachineInventory(input: ProductLoadoutInput | null | undefined): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/Machine/UpdateProductLoadout";
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
            return this.processUpdateMachineInventory(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdateMachineInventory(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>Observable.throw(e);
                }
            } else
                return <Observable<boolean>><any>Observable.throw(response_);
        });
    }

    protected processUpdateMachineInventory(response: HttpResponseBase): Observable<boolean> {
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
    // end process update inventory

    /**
     * @return Success
     */
    delete(id: string): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/Machine/Delete?";
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
    get(id: string): Observable<MachineDto> {
        let url_ = this.baseUrl + "/api/services/app/Machine/GetDetail?";
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
                    return <Observable<MachineDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<MachineDto>><any>Observable.throw(response_);
        });
    }

    protected processGet(response: HttpResponseBase): Observable<MachineDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? MachineDto.fromJS(resultData200) : new MachineDto();
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
        return Observable.of<MachineDto>(<any>null);
    }

    /**
     * @return Success
     */
    getAll(skipCount: number, maxResultCount: number, sorting: string): Observable<PagedResultDtoOfMachineDto> {
        let url_ = this.baseUrl + "/api/services/app/Machine/GetAll?";
        if (skipCount === undefined || skipCount === null)
            throw new Error("The parameter 'skipCount' must be defined and cannot be null.");
        else
            url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&";
        if (maxResultCount === undefined || maxResultCount === null)
            throw new Error("The parameter 'maxResultCount' must be defined and cannot be null.");
        else
            url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&";

        url_ += "sorting=" + encodeURIComponent("" + sorting) + "&";

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


    /**
     * @input (optional) 
     * @return Success
     */
    uploadLoadoutCSV(input: UploadLoadOutCSVDto | null | undefined): Observable<ImportLoadourCSVResultDto> {
        let url_ = this.baseUrl + "/api/services/app/Machine/UploadLoadoutCSV";
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
            return this.processUploadLoadout(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUploadLoadout(<any>response_);
                } catch (e) {
                    return <Observable<ImportLoadourCSVResultDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<ImportLoadourCSVResultDto>><any>Observable.throw(response_);
        });
    }

    protected processUploadLoadout(response: HttpResponseBase): Observable<ImportLoadourCSVResultDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? ImportLoadourCSVResultDto.fromJS(resultData200) : new ImportLoadourCSVResultDto();
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
        return Observable.of<ImportLoadourCSVResultDto>(<any>null);
    }

    /**
     * @return Success
     */
    getLoadoutItemStatus(id: string): Observable<MachineLoadoutItemStatusDto> {
        let url_ = this.baseUrl + "/api/services/app/Machine/GetLoadoutItemStatus?";
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
            return this.processGetLoadoutItemStatus(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetLoadoutItemStatus(<any>response_);
                } catch (e) {
                    return <Observable<MachineLoadoutItemStatusDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<MachineLoadoutItemStatusDto>><any>Observable.throw(response_);
        });
    }

    protected processGetLoadoutItemStatus(response: HttpResponseBase): Observable<MachineLoadoutItemStatusDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? MachineLoadoutItemStatusDto.fromJS(resultData200) : new MachineLoadoutItemStatusDto();
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
        return Observable.of<MachineLoadoutItemStatusDto>(<any>null);
    }


    /// process machine loadout
    sendCommandToMachine(input: SendRemoteCommandInput): Observable<SendRemoteCommandOutputDto> {
        let url_ = this.baseUrl + "/api/services/app/Machine/SendCommandToMachine";
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
            return this.processSendCommandToMachine(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processSendCommandToMachine(<any>response_);
                } catch (e) {
                    return <Observable<SendRemoteCommandOutputDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<SendRemoteCommandOutputDto>><any>Observable.throw(response_);
        });
    }

    protected processSendCommandToMachine(response: HttpResponseBase): Observable<SendRemoteCommandOutputDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? SendRemoteCommandOutputDto.fromJS(resultData200) : new SendRemoteCommandOutputDto();
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
        return Observable.of<SendRemoteCommandOutputDto>(<any>null);
    }

    // get machine errors
    getAllErrors(skipCount: number, maxResultCount: number): Observable<PagedResultDtoOfMachineErrorDto> {
        let url_ = this.baseUrl + "/api/services/app/MachineDiagnostic/GetAllMachineErrors?";
        if (skipCount === undefined || skipCount === null)
            throw new Error("The parameter 'skipCount' must be defined and cannot be null.");
        else
            url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&";
        if (maxResultCount === undefined || maxResultCount === null)
            throw new Error("The parameter 'maxResultCount' must be defined and cannot be null.");
        else
            url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&";
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
            return this.processGetAllErrors(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllErrors(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfMachineErrorDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<PagedResultDtoOfMachineErrorDto>><any>Observable.throw(response_);
        });
    }

    protected processGetAllErrors(response: HttpResponseBase): Observable<PagedResultDtoOfMachineErrorDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PagedResultDtoOfMachineErrorDto.fromJS(resultData200) : new PagedResultDtoOfMachineErrorDto();
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
        return Observable.of<PagedResultDtoOfMachineErrorDto>(<any>null);
    }


    // get machine status
    getAllMachineStatus(): Observable<PagedResultDtoOfMachineStatusDto> {
        let url_ = this.baseUrl + "/api/services/app/Machine/GetAllMachineStatus?";


        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).flatMap((response_: any) => {
            return this.processGetAllMachineStatus(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllMachineStatus(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfMachineStatusDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<PagedResultDtoOfMachineStatusDto>><any>Observable.throw(response_);
        });
    }

    protected processGetAllMachineStatus(response: HttpResponseBase): Observable<PagedResultDtoOfMachineStatusDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PagedResultDtoOfMachineStatusDto.fromJS(resultData200) : new PagedResultDtoOfMachineStatusDto();
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
        return Observable.of<PagedResultDtoOfMachineStatusDto>(<any>null);
    }

    /**
     * @return Success
     */
    getGetErrorSolutionAll(skipCount: number, maxResultCount: number): Observable<PagedResultDtoOfMachineErorSolutionDto> {
        let url_ = this.baseUrl + "/api/services/app/Machine/GetMachineErrorSolutionAll?";
        if (skipCount === undefined || skipCount === null)
            throw new Error("The parameter 'skipCount' must be defined and cannot be null.");
        else
            url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&";
        if (maxResultCount === undefined || maxResultCount === null)
            throw new Error("The parameter 'maxResultCount' must be defined and cannot be null.");
        else
            url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&";
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
            return this.processGetErrorSolutionAll(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetErrorSolutionAll(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfMachineErorSolutionDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<PagedResultDtoOfMachineErorSolutionDto>><any>Observable.throw(response_);
        });
    }

    protected processGetErrorSolutionAll(response: HttpResponseBase): Observable<PagedResultDtoOfMachineErorSolutionDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PagedResultDtoOfMachineErorSolutionDto.fromJS(resultData200) : new PagedResultDtoOfMachineErorSolutionDto();
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
        return Observable.of<PagedResultDtoOfMachineErorSolutionDto>(<any>null);
    }

    /**
    * @return Success
    */
    getAllMachinesForCombobox(cloudAPIUrl : string): Observable<ListResultDtoOfMachineComboboxDto> {
        let url_ = cloudAPIUrl + "api/services/app/Machine/GetMachinesForComboBox";

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).flatMap((response_: any) => {
            return this.processGetAllMachinesForCombobox(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllMachinesForCombobox(<any>response_);
                } catch (e) {
                    return <Observable<ListResultDtoOfMachineComboboxDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<ListResultDtoOfMachineComboboxDto>><any>Observable.throw(response_);
        });
    }

    protected processGetAllMachinesForCombobox(response: HttpResponseBase): Observable<ListResultDtoOfMachineComboboxDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? ListResultDtoOfMachineComboboxDto.fromJS(resultData200) : new ListResultDtoOfMachineComboboxDto();
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
        return Observable.of<ListResultDtoOfMachineComboboxDto>(<any>null);
    }

}

export class UploadLoadOutCSVDto {
    fileUrl: string;
    fileContent: string;
    machineID: string;
}

export interface ICreateMachineDto {
    name: string;
    id: string;
    cashlessTerminalId: string;
}



export class CreateMachineDto implements ICreateMachineDto {
    name: string;
    id: string;
    cashlessTerminalId: string;
    constructor(data?: ICreateMachineDto) {
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
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CreateMachineDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateMachineDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["cashlessTerminalId"] = this.cashlessTerminalId;
        data["id"] = this.id;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new CreateMachineDto();
        result.init(json);
        return result;
    }
}

export class SendRemoteCommandInput {
    MachineID: string;
    CommandName: string;
    CommandArgs: string;

}


export class SendRemoteCommandOutputDto {
    IsSuccess: Boolean | undefined;
    Message: string[] | undefined;

    constructor(data?: SendRemoteCommandOutputDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        console.log(data);
        if (data) {
            this.IsSuccess = data["isSuccess"];
            this.Message = data["message"];
        }
    }

    static fromJS(data: any): SendRemoteCommandOutputDto {
        //console.log(data);
        data = typeof data === 'object' ? data : {};
        let result = new SendRemoteCommandOutputDto();
        result.init(data);
        return result;
    }
}


//export interface ICreateMachineDto {
//    name: string;
//    id: string;
//    cashlessTerminalId: string;
//}


export class MachineDto implements IMachineDto {
    name: string;
    id: string | undefined;
    TenantId: number | undefined;
    TenantName: string | undefined;
    PlannedSessionInventories: PlannedSessionInventory[] | undefined;
    cashlessTerminalId: string;
    registeredAzureIoT: boolean;

    constructor(data?: IMachineDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            //console.log(data);
            this.name = data["name"];
            this.cashlessTerminalId = data["cashlessTerminalId"];
            this.id = data["id"];
            this.registeredAzureIoT = data["registeredAzureIoT"];

            if (data["plannedSessionInventories"] != undefined && data["plannedSessionInventories"] != null) {
                this.PlannedSessionInventories = new Array();

                for (let i = 0; i < data["plannedSessionInventories"].length; i++) {
                    // Do something
                    let item = new PlannedSessionInventory();
                    item.init(data["plannedSessionInventories"][i]);
                    this.PlannedSessionInventories.push(item);
                }
            }
        }
    }

    static fromJS(data: any): MachineDto {
        data = typeof data === 'object' ? data : {};
        let result = new MachineDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["cashlessTerminalId"] = this.cashlessTerminalId;
        data["id"] = this.id;
        data["registeredAzureIoT"] = this.registeredAzureIoT;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new MachineDto();
        result.init(json);
        return result;
    }
}

export class MachineErrorDto {

    machineId: string;
    machineName: string;
    machineErrorCode: string;
    message: string;
    solution: string;
    time: string;

    constructor(data?: IMachineDto) {
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
            this.machineErrorCode = data["machineErrorCode"];
            this.message = data["message"];
            this.solution = data["solution"];
            this.time = data["time"];

        }
    }

    static fromJS(data: any): MachineErrorDto {
        data = typeof data === 'object' ? data : {};
        let result = new MachineErrorDto();
        result.init(data);
        return result;
    }
}


export class DeviceStatusDto {

    Name: string;
    IsConnected: boolean;
    State: string;
    ErrorMessages: string;

    constructor(data?: DeviceStatusDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.IsConnected = data["isConnected"];
            this.ErrorMessages = data["errorMessages"];
            this.State = data["state"];
            this.Name = data["name"];
        }
    }

    static fromJS(data: any): DeviceStatusDto {
        data = typeof data === 'object' ? data : {};
        let result = new DeviceStatusDto();
        result.init(data);
        return result;
    }
}

export class MachineStatusDto {

    MachineId: string;
    VmcState: string;
    VmcType: string;
    MachineType: string;
    Name: string;
    Temperature: number;
    DispenseErrorCount: number;
    IsOffline: boolean;
    LastModified: Date;
    DeviceStatus: DeviceStatusDto[];

    constructor(data?: MachineStatusDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.MachineId = data["machineId"];
            this.VmcState = data["vmcState"];
            this.VmcType = data["vmcType"];
            this.MachineType = data["machineType"];
            this.Name = data["name"];
            this.Temperature = data["temperature"];
            this.IsOffline = data["isOffline"];
            this.LastModified = data["lastModified"];
            this.DispenseErrorCount = data["dispenseErrorCount"];
            if (data["deviceStatus"] != undefined && data["deviceStatus"] != null) {
                this.DeviceStatus = new Array();

                for (let i = 0; i < data["deviceStatus"].length; i++) {
                    let item = new DeviceStatusDto();
                    item.init(data["deviceStatus"][i]);
                    this.DeviceStatus.push(item);
                }
            }
        }
    }

    static fromJS(data: any): MachineStatusDto {
        data = typeof data === 'object' ? data : {};
        let result = new MachineStatusDto();
        result.init(data);
        return result;
    }
}


export class MachineLoadoutItemStatusDto implements IMachineLoadoutItemStatusDto {

    ItemsStatusDtos: LoadoutItemStatusDto[] | undefined = new Array();
    LineData: LoadoutItemStatusDto[][] = new Array();

    constructor(data?: IMachineLoadoutItemStatusDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {

            if (data["itemsStatusDtos"] != undefined && data["itemsStatusDtos"] != null) {
                this.ItemsStatusDtos = new Array();

                for (let i = 0; i < data["itemsStatusDtos"].length; i++) {
                    let item = new LoadoutItemStatusDto();
                    item.init(data["itemsStatusDtos"][i]);

                    let lineItem = Math.floor(item.ItemLocationNumber / 100);

                    if (this.LineData[lineItem] == undefined) {
                        this.LineData[lineItem] = new Array();
                    }
                    this.LineData[lineItem].push(item);

                    this.ItemsStatusDtos.push(item);
                }

            }
        }
    }

    static fromJS(data: any): MachineLoadoutItemStatusDto {
        console.log(data);
        data = typeof data === 'object' ? data : {};
        let result = new MachineLoadoutItemStatusDto();
        result.init(data);
        return result;
    }


}


export interface IMachineLoadoutItemStatusDto {
    ItemsStatusDtos: LoadoutItemStatusDto[] | undefined;
}


export function CopyObjectProperties(source, desc) {
    for (var prop in desc) {
        if (source.hasOwnProperty(prop)) {
            desc[prop] = source[prop];
        }
    }
}

export interface IMachineDto {
    name: string;
    cashlessTerminalId: string;
    registeredAzureIoT: boolean;
}


export class PlannedSessionInventory {
    id: string;
    session: InventorySession;
    items: ItemInventory[] | undefined;
    init(data?: any) {
        this.id = data["id"];
        this.session = new InventorySession();
        if (data["session"] != null) {
            CopyObjectProperties(data["session"], this.session);
            this.session = InventorySession.fromJS(data["session"]);
        }

        this.items = new Array();
        if (data["items"] != null) {
            for (let i = 0; i < data["items"].length; i++) {
                let item = ItemInventory.fromJS(data["items"][i]);
                this.items.push(item);
            }
        }
        this.items = this.items.sort(this.compare);
    }

    compare(a, b) {
        if (a.locationCode < b.locationCode)
            return -1;
        if (a.locationCode > b.locationCode)
            return 1;
        return 0;
    }
}


export class LoadoutItemStatusDto {
    ItemLocation: string;
    ItemLocationNumber: number;
    Quantity: number;
    HealthStatus: number;


    init(data?: any) {
        this.ItemLocation = data["itemLocation"];
        this.ItemLocationNumber = parseInt(this.ItemLocation);
        this.Quantity = data["quantity"];
        this.HealthStatus = data["healthStatus"];

    }
}


export class ItemInventory {
    sessionID: string | undefined;
    locationCode: number;
    productId: string | undefined;
    price: number | undefined;
    quantity: number | undefined;
    currentQuantity: number | undefined;
    // isBlank: boolean | undefined;
    ProductName: string = '';
    ImageLocation: string = '';
    isBlank() {
        return this.productId == null || this.productId == undefined;
    }

    init(data?: any) {
        if (data) {
            //console.log(data);
            this.sessionID = data["sessionId"];
            this.locationCode = data["locationCode"];
            this.productId = data["productId"];
            this.price = data["price"];
            this.quantity = data["quantity"];
            this.currentQuantity = data["currentQuantity"];
            this.ProductName = data["productName"];
            this.ImageLocation = data["imageLocation"];
        }
    }

    static fromJS(data: any): ItemInventory {
        data = typeof data === 'object' ? data : {};
        let result = new ItemInventory();
        result.init(data);
        return result;
    }
}

export class InventorySession {
    id: string;
    name: string;
    tenantId: number;
    sessionType: InventorySessionType;

    init(data?: any) {
        if (data) {
            //console.log(data);
            this.id = data["id"];
            this.name = data["name"];
            this.tenantId = data["tenantId"];
            this.sessionType = data["type"];

        }
    }

    static fromJS(data: any): InventorySession {
        data = typeof data === 'object' ? data : {};
        let result = new InventorySession();
        result.init(data);
        return result;
    }
}

export class ProductLoadoutInput {
    MachineId: string;
    SessionId: string;
    ProductId: string;
    Price: number;
    Quantity: number;
    LocationCode: number;
}

export enum InventorySessionType {
    None = 100,
    S = 0,
    A = 1,
    B = 2,
    C = 3
}

export class PagedResultDtoOfMachineDto implements IPagedResultDtoOfMachineDto {
    totalCount: number | undefined;
    items: MachineDto[] | undefined;

    constructor(data?: IPagedResultDtoOfMachineDto) {
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
                    this.items.push(MachineDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfMachineDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfMachineDto();
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
        let result = new PagedResultDtoOfMachineDto();
        result.init(json);
        return result;
    }
}

export interface IPagedResultDtoOfMachineDto {
    totalCount: number | undefined;
    items: MachineDto[] | undefined;
}


export class PagedResultDtoOfMachineErrorDto {
    totalCount: number | undefined;
    items: MachineErrorDto[] | undefined;

    constructor(data?: PagedResultDtoOfMachineErrorDto) {
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
                    this.items.push(MachineErrorDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfMachineErrorDto {
        console.log(data);
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfMachineErrorDto();
        result.init(data);
        return result;
    }
}



export class PagedResultDtoOfMachineStatusDto {
    totalCount: number | undefined;
    items: MachineStatusDto[] | undefined;

    constructor(data?: PagedResultDtoOfMachineStatusDto) {
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
                    this.items.push(MachineStatusDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfMachineStatusDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfMachineStatusDto();
        result.init(data);
        return result;
    }
}


export class ImportLoadourCSVResultDto {
    IsSuccess: Boolean | undefined;
    Message: string[] | undefined;

    constructor(data?: ImportLoadourCSVResultDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.IsSuccess = data["isSuccess"];
            this.Message = data["message"];
        }
    }

    static fromJS(data: any): ImportLoadourCSVResultDto {
        console.log(data);
        data = typeof data === 'object' ? data : {};
        let result = new ImportLoadourCSVResultDto();
        result.init(data);
        return result;
    }
}
// end Machine
export class PagedResultDtoOfMachineErorSolutionDto implements IPagedResultDtoOfMachineErorSolutionDto {
    totalCount: number | undefined;
    items: any | undefined;

    constructor(data?: IPagedResultDtoOfMachineErorSolutionDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        console.log(data);
        if (data) {
            this.totalCount = data["totalCount"];
            this.items = data["items"]
        }
    }

    static fromJS(data: any): PagedResultDtoOfMachineErorSolutionDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfMachineErorSolutionDto();
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
        let result = new PagedResultDtoOfMachineErorSolutionDto();
        result.init(json);
        return result;
    }
}

export interface IPagedResultDtoOfMachineErorSolutionDto {
    totalCount: number | undefined;
    items: any | undefined;
}
//Machine for combo box
export interface IMachineComboboxDto {
    name: string;
    id: string | undefined;
}
export class MachineComboboxDto implements IMachineComboboxDto {
    name: string;
    id: string | undefined;

    constructor(data?: IMachineComboboxDto) {
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
        }
    }

    static fromJS(data: any): MachineComboboxDto {
        data = typeof data === 'object' ? data : {};
        let result = new MachineComboboxDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["id"] = this.id;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new MachineComboboxDto();
        result.init(json);
        return result;
    }
}

export class ListResultDtoOfMachineComboboxDto {
    items: MachineComboboxDto[] | undefined;

    constructor(data?: ListResultDtoOfMachineComboboxDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["items"] && data["items"].constructor === Array) {
                this.items = [];
                for (let item of data["items"])
                    this.items.push(MachineComboboxDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfMachineComboboxDto {
        console.log(data);
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfMachineComboboxDto();
        result.init(data);
        return result;
    }
}

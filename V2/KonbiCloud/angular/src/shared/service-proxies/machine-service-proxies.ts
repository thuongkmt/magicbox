import * as moment from 'moment';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
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
    initializeMachine(id: string): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/Machine/initializeMachine";
        // if (id === undefined || id === null)
        //     throw new Error("The parameter 'id' must be defined and cannot be null.");
        // else
        //     url_ += "Id=" + encodeURIComponent("" + id) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            body:{id:id},
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        // let options_: any = {
        //     body: content_,
        //     observe: "response",
        //     responseType: "blob",
        //     headers: new HttpHeaders({
        //         "Content-Type": "application/json",
        //         "Accept": "application/json"
        //     })
        // };

        return this.http.request("post", url_, options_).flatMap((response_: any) => {
            return this.processinitializeMachine(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processinitializeMachine(<any>response_);
                } catch (e) {
                    return <Observable<void>><any>Observable.throw(e);
                }
            } else
                return <Observable<void>><any>Observable.throw(response_);
        });
    }

    protected processinitializeMachine(response: HttpResponseBase): Observable<void> {
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
    getMachineDetail(id: string): Observable<MachineDto> {
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

     /**
     * @return Success
     */
    GetAllUnintialized(skipCount: number, maxResultCount: number, sorting: string): Observable<PagedResultDtoOfMachineDto> {
        let url_ = this.baseUrl + "/api/services/app/Machine/GetAllUnintialized?";
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
            return this.processGetAllUnintialized(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllUnintialized(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfMachineDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<PagedResultDtoOfMachineDto>><any>Observable.throw(response_);
        });
    }

    protected processGetAllUnintialized(response: HttpResponseBase): Observable<PagedResultDtoOfMachineDto> {
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
     * @param machineId (optional) 
     * @return Success
     */
    getMachineDevices(machineId: string | null | undefined): Observable<Device[]> {
        let url_ = this.baseUrl + "/api/services/app/Machine/GetMachineDevices?";
        if (machineId !== undefined)
            url_ += "machineId=" + encodeURIComponent("" + machineId) + "&"; 
        url_ = url_.replace(/[?&]$/, "");

        let options_ : any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
            return this.processGetMachineDevices(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetMachineDevices(<any>response_);
                } catch (e) {
                    return <Observable<Device[]>><any>_observableThrow(e);
                }
            } else
                return <Observable<Device[]>><any>_observableThrow(response_);
        }));
    }

     protected processGetMachineDevices(response: HttpResponseBase): Observable<Device[]> {
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
                    result200.push(Device.fromJS(item));
            }
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<Device[]>(<any>null);
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


    /// process Machine loadout
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

    // get Machine errors
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

    // get Machine status
    getAllMachineStatus(input: MachineStatusDto | null | undefined): Observable<PagedResultDtoOfMachineStatusDto> {
        let url_ = this.baseUrl + "/api/services/app/Machine/QueryMachineStatus";
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
    getAllMachinesForCombobox(): Observable<ListResultDtoOfMachineComboboxDto> {
        let url_ = this.baseUrl + "/api/services/app/Machine/GetMachinesForComboBox";

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

    updateMachineLoadoutSize(input: MachineLoadoutInput): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/Machine/UpdateMachineLoadoutSize";
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
            return this.processUpdateMachineLoadoutSize(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdateMachineLoadoutSize(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>Observable.throw(e);
                }
            } else
                return <Observable<boolean>><any>Observable.throw(response_);
        });
    }

    protected processUpdateMachineLoadoutSize(response: HttpResponseBase): Observable<boolean> {
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
        return Observable.of<boolean>(<boolean>null);
    }

}

export class UploadLoadOutCSVDto {
    fileUrl: string;
    fileContent: string;
    MachineID: string;
}

export interface ICreateMachineDto {
    name: string;
    id: string;
    cashlessTerminalId: string;
}



export class Device implements IDevice {
    code!: string | undefined;
    port!: string | undefined;
    name!: string | undefined;
    status!: string | undefined;
    state!: string | undefined;
    custonInfoesJson!: string | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: string | undefined;

    constructor(data?: IDevice) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.code = data["code"];
            this.port = data["port"];
            this.name = data["name"];
            this.status = data["status"];
            this.state = data["state"];
            this.custonInfoesJson = data["custonInfoesJson"];
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

    static fromJS(data: any): Device {
        data = typeof data === 'object' ? data : {};
        let result = new Device();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["code"] = this.code;
        data["port"] = this.port;
        data["name"] = this.name;
        data["status"] = this.status;
        data["state"] = this.state;
        data["custonInfoesJson"] = this.custonInfoesJson;
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

export interface IDevice {
    code: string | undefined;
    port: string | undefined;
    name: string | undefined;
    status: string | undefined;
    state: string | undefined;
    custonInfoesJson: string | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: string | undefined;
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
    cashlessTerminalId: string;
    // registeredAzureIoT: boolean;
    isOffline:boolean;
    isInitialized:boolean;
    machineType:string;

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
            // this.registeredAzureIoT = data["registeredAzureIoT"];
            this.isOffline = data["isOffline"];
            this.isInitialized = data["isInitialized"];
            this.machineType = data["machineType"];
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
        // data["registeredAzureIoT"] = this.registeredAzureIoT;
        data["isOffline"]=this.isOffline;
        data["isInitialized"]=this.isInitialized;
        data["machineType"]=this.machineType;
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

    MachineId: string;
    MachineName: string;
    MachineErrorCode: string;
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
            this.MachineId = data["MachineId"];
            this.MachineName = data["MachineName"];
            this.MachineErrorCode = data["MachineErrorCode"];
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
    MachineName: string;
    Temperature: string;
    DoorState: boolean;
    DoorLastClosed: string;
    MachineIp: string;
    Uptime: string;
    Cpu: string;
    Memory: string;
    Hdd: string;
    PaymentState: string;
    TopupTime: string;
    LastTagTakenOut: string;
    LastTagPutIn: string;
    IsOnline: boolean;

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
            this.MachineName = data["machineName"];
            this.DoorState = data["doorState"];
            this.DoorLastClosed = data["doorLastClosed"];
            this.Temperature = data["temperature"];
            this.MachineIp = data["machineIp"];
            this.Uptime = data["uptime"];
            this.Cpu = data["cpu"];
            this.Memory = data["memory"];
            this.Hdd = data["hdd"];
            this.PaymentState = data["paymentState"];
            this.TopupTime = data["topupTime"];
            this.LastTagTakenOut = data["lastTagTakenOut"];
            this.LastTagPutIn = data["lastTagPutIn"];
            this.IsOnline = data["isOnline"];
        }
    }

    static fromJS(data: any): MachineStatusDto {
        data = typeof data === 'object' ? data : {};
        let result = new MachineStatusDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["machineId"] = this.MachineId;
        data["machineName"] = this.MachineName;
        data["doorState"] = this.DoorState;
        data["doorLastClosed"] = this.DoorLastClosed;
        data["temperature"] = this.Temperature;
        data["machineIp"] = this.MachineIp;
        data["uptime"] = this.Uptime;
        data["cpu"] = this.Cpu;
        data["memory"] = this.Memory;
        data["hdd"] = this.Hdd;
        data["paymentState"] = this.PaymentState;
        data["topupTime"] = this.TopupTime;
        data["lastTagTakenOut"] = this.LastTagTakenOut;
        data["lastTagPutIn"] = this.LastTagPutIn;
        data["isOnline"] = this.IsOnline;
        return data; 
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
    // registeredAzureIoT: boolean;
    isOffline:boolean;
    isInitialized:boolean;
    machineType:string;
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
    isBlank: boolean | undefined;
    ProductName: string = '';
    ImageLocation: string = '';
    productSKU: string = '';
    isShow: boolean;

    hasProduct() {
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
            this.isBlank = data["isBlank"];   
            this.productSKU = data["productSKU"];
            this.isShow = data["isShow"];
        }
    }

    static fromJS(data: any): ItemInventory {
        data = typeof data === 'object' ? data : {};
        let result = new ItemInventory();
        result.init(data);
        return result;
    }
 }

// export class InventorySession {
//     id: string;
//     name: string;
//     tenantId: number;
//     sessionType: InventorySessionType;

//     init(data?: any) {
//         if (data) {
//             //console.log(data);
//             this.id = data["id"];
//             this.name = data["name"];
//             this.tenantId = data["tenantId"];
//             this.sessionType = data["type"];

//         }
//     }

//     static fromJS(data: any): InventorySession {
//         data = typeof data === 'object' ? data : {};
//         let result = new InventorySession();
//         result.init(data);
//         return result;
//     }
// }

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
export class MachineLoadoutInput{
    machineId: string;
    sessionId: string;
    rows: number;
    columns: number;
}
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
import { DiscDto } from './service-proxies';

import * as moment from 'moment';


@Injectable()
export class TransactionServiceProxy {
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
    getAllTransactions(
        dateFilter: string | null | undefined,
        sessionFilter: string | null | undefined,
        statesFilter: string | null | undefined,
        transactionType: number | null | undefined,
        sorting: string | null | undefined,
        maxResultCount: number | null | undefined,
        skipCount: number | null | undefined
    ): Observable<PagedResultDtoOfTransactionDto> {
        let url_ = this.baseUrl + "/api/services/app/Transaction/GetAllTransactions?";
        if (dateFilter !== undefined)
            url_ += "DateFilter=" + encodeURIComponent("" + dateFilter) + "&";
        if (sessionFilter !== undefined)
            url_ += "SessionFilter=" + encodeURIComponent("" + sessionFilter) + "&";
        if (statesFilter !== undefined)
            url_ += "StateFilter=" + encodeURIComponent("" + statesFilter) + "&";
        if (transactionType !== undefined)
            url_ += "TransactionType=" + encodeURIComponent("" + transactionType) + "&";
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

        return this.http.request("get", url_, options_).flatMap((response_: any) => {
            return this.processGetAllTransactions(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllTransactions(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfTransactionDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<PagedResultDtoOfTransactionDto>><any>Observable.throw(response_);
        });
    }

    protected processGetAllTransactions(response: HttpResponseBase): Observable<PagedResultDtoOfTransactionDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PagedResultDtoOfTransactionDto.fromJS(resultData200) : new PagedResultDtoOfTransactionDto();
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
        return Observable.of<PagedResultDtoOfTransactionDto>(<any>null);
    }
}


export class TransactionDto {
    id: string | undefined;
    tranCode: string;
    buyer: string | undefined;
    paymentTime: Date;
    amount: number;
    platesQuantity: number;
    states: string;
    dishes: any;
    machine: string;
    session: string;
    transactionId: string;
    beginTranImage: string;
    endTranImage: string;

    constructor(data?: TransactionDto) {
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
            this.tranCode = data["tranCode"];
            this.buyer = data["buyer"];
            this.paymentTime = data["paymentTime"];
            this.amount = data["amount"];
            this.platesQuantity = data["platesQuantity"];
            this.states = data["states"];
            this.dishes = data["dishes"];
            this.machine = data["machine"];
            this.session = data["session"];
            this.transactionId = data["transactionId"];
            this.beginTranImage = data["beginTranImage"];
            this.endTranImage = data["endTranImage"];
        }
    }

    static fromJS(data: any): TransactionDto {
        data = typeof data === 'object' ? data : {};
        let result = new TransactionDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["tranCode"] = this.tranCode;
        data["buyer"] = this.buyer;
        data["paymentTime"] = this.paymentTime;
        data["amount"] = this.amount;
        data["platesQuantity"] = this.platesQuantity;
        data["states"] = this.states;
        data["dishes"] = this.dishes;
        data["machine"] = this.machine;
        data["session"] = this.session;
        data["transactionId"] = this.transactionId;
        data["beginTranImage"] = this.beginTranImage;
        data["endTranImage"] = this.endTranImage;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new TransactionDto();
        result.init(json);
        return result;
    }
}

export class DishTransaction {
    disc: DiscDto;
    amount: number;

    constructor(data?: TransactionDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.disc = data["disc"];
            this.amount = data["amount"];
        }
    }

    static fromJS(data: any): TransactionDto {
        data = typeof data === 'object' ? data : {};
        let result = new TransactionDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["disc"] = this.disc;
        data["amount"] = this.amount;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new TransactionDto();
        result.init(json);
        return result;
    }
}

export interface IPagedResultDtoOfTransactionDto {
    totalCount: number | undefined;
    items: TransactionDto[] | undefined;
}

export class PagedResultDtoOfTransactionDto implements IPagedResultDtoOfTransactionDto {
    totalCount: number | undefined;
    items: TransactionDto[] | undefined;

    constructor(data?: IPagedResultDtoOfTransactionDto) {
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
                    this.items.push(TransactionDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfTransactionDto {
        data = typeof data === 'object' ? data : {};
        //console.log(data);
        let result = new PagedResultDtoOfTransactionDto();
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
        let result = new PagedResultDtoOfTransactionDto();
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
            reader.onload = function () {
                observer.next(this.result);
                observer.complete();
            }
            reader.readAsText(blob);
        }
    });
}

function throwException(message: string, status: number, response: string, headers: { [key: string]: any; }, result?: any): Observable<any> {
    if (result !== null && result !== undefined)
        return Observable.throw(result);
    else
        return Observable.throw(new ApiServiceProxies.SwaggerException(message, status, response, headers, null));
}

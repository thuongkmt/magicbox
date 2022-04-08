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
     * @param sessionFilter (optional) 
     * @param stateFilter (optional) 
     * @param transactionType (optional) 
     * @param fromDate (optional) 
     * @param toDate (optional) 
     * @param machineFilter (optional) 
     * @param cardLabel (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getAllTransactions(sessionFilter: string | null | undefined, stateFilter: string | null | undefined, transactionType: number | null | undefined, fromDate: string | null | undefined, toDate: string | null | undefined, machineFilter: string | null | undefined, cardLabel: string | null | undefined, sorting: string | null | undefined, skipCount: number | null | undefined, maxResultCount: number | null | undefined): Observable<PagedResultDtoOfTransactionDto> {
        let url_ = this.baseUrl + "/api/services/app/Transaction/GetAllTransactions?";
        if (sessionFilter !== undefined)
            url_ += "SessionFilter=" + encodeURIComponent("" + sessionFilter) + "&"; 
        if (stateFilter !== undefined)
            url_ += "StateFilter=" + encodeURIComponent("" + stateFilter) + "&"; 
        if (transactionType !== undefined)
            url_ += "TransactionType=" + encodeURIComponent("" + transactionType) + "&"; 
        if (fromDate !== undefined)
            url_ += "FromDate=" + encodeURIComponent("" + fromDate) + "&"; 
        if (toDate !== undefined)
            url_ += "ToDate=" + encodeURIComponent("" + toDate) + "&"; 
        if (machineFilter !== undefined)
            url_ += "MachineFilter=" + encodeURIComponent("" + machineFilter) + "&"; 
        if (cardLabel !== undefined)
            url_ += "CardLabel=" + encodeURIComponent("" + cardLabel) + "&"; 
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
            return this.processGetAllTransactions(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllTransactions(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfTransactionDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfTransactionDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetAllTransactions(response: HttpResponseBase): Observable<PagedResultDtoOfTransactionDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? PagedResultDtoOfTransactionDto.fromJS(resultData200) : new PagedResultDtoOfTransactionDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfTransactionDto>(<any>null);
    }

    /**
     * @param sessionFilter (optional) 
     * @param stateFilter (optional) 
     * @param transactionType (optional) 
     * @param fromDate (optional) 
     * @param toDate (optional) 
     * @param machineFilter (optional) 
     * @param cardLabel (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getAllTransactionFinanceReport(sessionFilter: string | null | undefined, stateFilter: string | null | undefined, transactionType: number | null | undefined, fromDate: string | null | undefined, toDate: string | null | undefined, machineFilter: string | null | undefined, cardLabel: string | null | undefined, sorting: string | null | undefined, skipCount: number | null | undefined, maxResultCount: number | null | undefined): Observable<PagedResultDtoOfTransactionFinanceReportDto> {
        let url_ = this.baseUrl + "/api/services/app/Transaction/GetAllTransactionFinanceReport?";
        if (sessionFilter !== undefined)
            url_ += "SessionFilter=" + encodeURIComponent("" + sessionFilter) + "&"; 
        if (stateFilter !== undefined)
            url_ += "StateFilter=" + encodeURIComponent("" + stateFilter) + "&"; 
        if (transactionType !== undefined)
            url_ += "TransactionType=" + encodeURIComponent("" + transactionType) + "&"; 
        if (fromDate !== undefined)
            url_ += "FromDate=" + encodeURIComponent("" + fromDate) + "&"; 
        if (toDate !== undefined)
            url_ += "ToDate=" + encodeURIComponent("" + toDate) + "&"; 
        if (machineFilter !== undefined)
            url_ += "MachineFilter=" + encodeURIComponent("" + machineFilter) + "&"; 
        if (cardLabel !== undefined)
            url_ += "CardLabel=" + encodeURIComponent("" + cardLabel) + "&"; 
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
            return this.processGetAllTransactionFinanceReport(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllTransactionFinanceReport(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfTransactionFinanceReportDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfTransactionFinanceReportDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetAllTransactionFinanceReport(response: HttpResponseBase): Observable<PagedResultDtoOfTransactionFinanceReportDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? PagedResultDtoOfTransactionFinanceReportDto.fromJS(resultData200) : new PagedResultDtoOfTransactionFinanceReportDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfTransactionFinanceReportDto>(<any>null);
    }

    /**
     * @param sessionFilter (optional) 
     * @param stateFilter (optional) 
     * @param transactionType (optional) 
     * @param fromDate (optional) 
     * @param toDate (optional) 
     * @param machineFilter (optional) 
     * @param cardLabel (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getAllTransactionItemsReport(sessionFilter: string | null | undefined, stateFilter: string | null | undefined, transactionType: number | null | undefined, fromDate: string | null | undefined, toDate: string | null | undefined, machineFilter: string | null | undefined, cardLabel: string | null | undefined, sorting: string | null | undefined, skipCount: number | null | undefined, maxResultCount: number | null | undefined): Observable<PagedResultDtoOfTransactionItemsReportDto> {
        let url_ = this.baseUrl + "/api/services/app/Transaction/GetAllTransactionItemsReport?";
        if (sessionFilter !== undefined)
            url_ += "SessionFilter=" + encodeURIComponent("" + sessionFilter) + "&"; 
        if (stateFilter !== undefined)
            url_ += "StateFilter=" + encodeURIComponent("" + stateFilter) + "&"; 
        if (transactionType !== undefined)
            url_ += "TransactionType=" + encodeURIComponent("" + transactionType) + "&"; 
        if (fromDate !== undefined)
            url_ += "FromDate=" + encodeURIComponent("" + fromDate) + "&"; 
        if (toDate !== undefined)
            url_ += "ToDate=" + encodeURIComponent("" + toDate) + "&"; 
        if (machineFilter !== undefined)
            url_ += "MachineFilter=" + encodeURIComponent("" + machineFilter) + "&"; 
        if (cardLabel !== undefined)
            url_ += "CardLabel=" + encodeURIComponent("" + cardLabel) + "&"; 
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
            return this.processGetAllTransactionItemsReport(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllTransactionItemsReport(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfTransactionItemsReportDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfTransactionItemsReportDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetAllTransactionItemsReport(response: HttpResponseBase): Observable<PagedResultDtoOfTransactionItemsReportDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? PagedResultDtoOfTransactionItemsReportDto.fromJS(resultData200) : new PagedResultDtoOfTransactionItemsReportDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfTransactionItemsReportDto>(<any>null);
    }



}


export class PagedResultDtoOfTransactionItemsReportDto implements IPagedResultDtoOfTransactionItemsReportDto {
    totalCount!: number | undefined;
    items!: TransactionItemsReportDto[] | undefined;

    constructor(data?: IPagedResultDtoOfTransactionItemsReportDto) {
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
                    this.items.push(TransactionItemsReportDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfTransactionItemsReportDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfTransactionItemsReportDto();
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

export interface IPagedResultDtoOfTransactionItemsReportDto {
    totalCount: number | undefined;
    items: TransactionItemsReportDto[] | undefined;
}

export class TransactionItemsReportDto implements ITransactionItemsReportDto {
    machine!: string | undefined;
    transactionId!: string | undefined;
    dateTime!: string | undefined;
    category!: string | undefined;
    productName!: string | undefined;
    expireDate!: string | undefined;
    sku!: string | undefined;
    tagId!: string | undefined;
    productUnitPrice!: number | undefined;
    productDiscountPrice!: number | undefined;
    transactionStatus!: string | undefined;
    paymentType!: string | undefined;
    cardId!: string | undefined;
    amountPaid!: number | undefined;

    constructor(data?: ITransactionItemsReportDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.machine = data["machine"];
            this.transactionId = data["transactionId"];
            this.dateTime = data["dateTime"];
            this.category = data["category"];
            this.productName = data["productName"];
            this.expireDate = data["expireDate"];
            this.sku = data["sku"];
            this.tagId = data["tagId"];
            this.productUnitPrice = data["productUnitPrice"];
            this.productDiscountPrice = data["productDiscountPrice"];
            this.transactionStatus = data["transactionStatus"];
            this.paymentType = data["paymentType"];
            this.cardId = data["cardId"];
            this.amountPaid = data["amountPaid"];
        }
    }

    static fromJS(data: any): TransactionItemsReportDto {
        data = typeof data === 'object' ? data : {};
        let result = new TransactionItemsReportDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["machine"] = this.machine;
        data["transactionId"] = this.transactionId;
        data["dateTime"] = this.dateTime;
        data["category"] = this.category;
        data["productName"] = this.productName;
        data["expireDate"] = this.expireDate;
        data["sku"] = this.sku;
        data["tagId"] = this.tagId;
        data["productUnitPrice"] = this.productUnitPrice;
        data["productDiscountPrice"] = this.productDiscountPrice;
        data["transactionStatus"] = this.transactionStatus;
        data["paymentType"] = this.paymentType;
        data["cardId"] = this.cardId;
        data["amountPaid"] = this.amountPaid;
        return data;
    }
}

export interface ITransactionItemsReportDto {
    machine: string | undefined;
    transactionId: string | undefined;
    dateTime: string | undefined;
    category: string | undefined;
    productName: string | undefined;
    expireDate: string | undefined;
    sku: string | undefined;
    tagId: string | undefined;
    productUnitPrice: number | undefined;
    productDiscountPrice: number | undefined;
    transactionStatus: string | undefined;
    paymentType: string | undefined;
    cardId: string | undefined;
    amountPaid: number | undefined;
}


export class PagedResultDtoOfTransactionFinanceReportDto implements IPagedResultDtoOfTransactionFinanceReportDto {
    totalCount!: number | undefined;
    items!: TransactionFinanceReportDto[] | undefined;

    constructor(data?: IPagedResultDtoOfTransactionFinanceReportDto) {
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
                    this.items.push(TransactionFinanceReportDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfTransactionFinanceReportDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfTransactionFinanceReportDto();
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

export interface IPagedResultDtoOfTransactionFinanceReportDto {
    totalCount: number | undefined;
    items: TransactionFinanceReportDto[] | undefined;
}

export class TransactionFinanceReportDto implements ITransactionFinanceReportDto {
    machine!: string | undefined;
    transactionId!: string | undefined;
    dateTime!: string | undefined;
    quantity!: number | undefined;
    transactionStatus!: string | undefined;
    paymentType!: string | undefined;
    cardId!: string | undefined;
    depositCollected!: number | undefined;
    amountPaid!: number | undefined;
    amountRefunded!: number | undefined;

    constructor(data?: ITransactionFinanceReportDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.machine = data["machine"];
            this.transactionId = data["transactionId"];
            this.dateTime = data["dateTime"];
            this.quantity = data["quantity"];
            this.transactionStatus = data["transactionStatus"];
            this.paymentType = data["paymentType"];
            this.cardId = data["cardId"];
            this.depositCollected = data["depositCollected"];
            this.amountPaid = data["amountPaid"];
            this.amountRefunded = data["amountRefunded"];
        }
    }

    static fromJS(data: any): TransactionFinanceReportDto {
        data = typeof data === 'object' ? data : {};
        let result = new TransactionFinanceReportDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["machine"] = this.machine;
        data["transactionId"] = this.transactionId;
        data["dateTime"] = this.dateTime;
        data["quantity"] = this.quantity;
        data["transactionStatus"] = this.transactionStatus;
        data["paymentType"] = this.paymentType;
        data["cardId"] = this.cardId;
        data["depositCollected"] = this.depositCollected;
        data["amountPaid"] = this.amountPaid;
        data["amountRefunded"] = this.amountRefunded;
        return data;
    }
}

export interface ITransactionFinanceReportDto {
    machine: string | undefined;
    transactionId: string | undefined;
    dateTime: string | undefined;
    quantity: number | undefined;
    transactionStatus: string | undefined;
    paymentType: string | undefined;
    cardId: string | undefined;
    depositCollected: number | undefined;
    amountPaid: number | undefined;
    amountRefunded: number | undefined;
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
    products: any;
    transactionDetails: any;
    machine: string;
    session: string;
    transactionId: string;
    beginTranImage: string;
    endTranImage: string;
    paidAmount: number;
    cardLabel: string;
    cardNumber: string;
    tagId: string;

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
            this.products = data["products"];
            this.transactionDetails = data["transactionDetails"];
            this.machine = data["machine"];
            this.session = data["session"];
            this.transactionId = data["transactionId"];
            this.beginTranImage = data["beginTranImage"];
            this.endTranImage = data["endTranImage"];
            this.paidAmount = data["paidAmount"];
            this.cardLabel = data["cardLabel"];
            this.cardNumber = data["cardNumber"];
            this.tagId = data["tagId"];
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
        data["products"] = this.products;
        data["transactionDetails"] = this.transactionDetails;
        data["machine"] = this.machine;
        data["session"] = this.session;
        data["transactionId"] = this.transactionId;
        data["beginTranImage"] = this.beginTranImage;
        data["endTranImage"] = this.endTranImage;
        data["paidAmount"] = this.paidAmount;
        data["cardLabel"] = this.cardLabel;
        data["cardNumber"] = this.cardNumber;
        data["tagId"] = this.tagId;
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
        //return Observable.throw(new ApiServiceProxies.SwaggerException(message, status, response, headers, null));
        throwException(message, status, response, headers);
}

import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { blobToText, throwException } from './service-base';
import { ListResultDtoOfPermissionDto, API_BASE_URL } from './service-proxies';
import * as moment from 'moment';
import { url } from 'inspector';

@Injectable()
export class DashboardServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @param salesDatePeriod (optional) 
     * @return Success
     */
    getDashboardData(salesDatePeriod: SalesDatePeriod | null | undefined): Observable<GetDashboardDataOutput> {
        let url_ = this.baseUrl + "/api/services/app/Dashboard/GetDashboardData?";
        if (salesDatePeriod !== undefined)
            url_ += "SalesDatePeriod=" + encodeURIComponent("" + salesDatePeriod) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetDashboardData(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetDashboardData(<any>response_);
                } catch (e) {
                    return <Observable<GetDashboardDataOutput>><any>_observableThrow(e);
                }
            } else
                return <Observable<GetDashboardDataOutput>><any>_observableThrow(response_);
        }));
    }

    protected processGetDashboardData(response: HttpResponseBase): Observable<GetDashboardDataOutput> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? GetDashboardDataOutput.fromJS(resultData200) : new GetDashboardDataOutput();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<GetDashboardDataOutput>(<any>null);
    }

    /**
     * @param salesDatePeriod (optional) 
     * @return Success
     */
    getSalesSummary(salesDatePeriod: SalesDatePeriod | null | undefined): Observable<GetSalesSummaryOutput> {
        let url_ = this.baseUrl + "/api/services/app/Dashboard/GetSalesData?";
        if (salesDatePeriod !== undefined)
            url_ += "SalesDatePeriod=" + encodeURIComponent("" + salesDatePeriod) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            // body: salesDatePeriod,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetSalesSummary(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetSalesSummary(<any>response_);
                } catch (e) {
                    return <Observable<GetSalesSummaryOutput>><any>_observableThrow(e);
                }
            } else
                return <Observable<GetSalesSummaryOutput>><any>_observableThrow(response_);
        }));
    }

    protected processGetSalesSummary(response: HttpResponseBase): Observable<GetSalesSummaryOutput> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? GetSalesSummaryOutput.fromJS(resultData200) : new GetSalesSummaryOutput();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<GetSalesSummaryOutput>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    getSessionStats(): Observable<GetSessionStatOutput> {
        let url_ = this.baseUrl + "/api/services/app/Dashboard/GetSessionStatData";
        // if (input !== undefined)
        //     url_ += "input=" + encodeURIComponent("" + input) + "&"; 
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetSessionStats(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetSessionStats(<any>response_);
                } catch (e) {
                    return <Observable<GetSessionStatOutput>><any>_observableThrow(e);
                }
            } else
                return <Observable<GetSessionStatOutput>><any>_observableThrow(response_);
        }));
    }

    protected processGetSessionStats(response: HttpResponseBase): Observable<GetSessionStatOutput> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? GetSessionStatOutput.fromJS(resultData200) : new GetSessionStatOutput();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<GetSessionStatOutput>(<any>null);
    }


    /**
 * @param input (optional) 
 * @return Success
 */
    getMachineStats(): Observable<GetMachineStatOutput> {
        let url_ = this.baseUrl + "/api/services/app/Dashboard/GetSaleByMachineData";
        // if (input !== undefined)
        //     url_ += "input=" + encodeURIComponent("" + input) + "&"; 
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetMachineStats(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetMachineStats(<any>response_);
                } catch (e) {
                    return <Observable<GetMachineStatOutput>><any>_observableThrow(e);
                }
            } else
                return <Observable<GetMachineStatOutput>><any>_observableThrow(response_);
        }));
    }

    protected processGetMachineStats(response: HttpResponseBase): Observable<GetMachineStatOutput> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? GetMachineStatOutput.fromJS(resultData200) : new GetMachineStatOutput();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<GetMachineStatOutput>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    getTransactionForToday(): Observable<TransactionForToday[]> {
        let url_ = this.baseUrl + "/api/services/app/Dashboard/GetTransactionForToday";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).flatMap((response_: any) => {
            return this.processGetTransactionForToday(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetTransactionForToday(<any>response_);
                } catch (e) {
                    return <Observable<TransactionForToday[]>><any>Observable.throw(e);
                }
            } else
                return <Observable<TransactionForToday[]>><any>Observable.throw(response_);
        });
    }

    protected processGetTransactionForToday(response: HttpResponseBase): Observable<TransactionForToday[]> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);                
                result200 = resultData200 ? resultData200 : new Array<TransactionForToday>();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<TransactionForToday[]>(<any>null);
    }

}

export enum SalesDatePeriod {
    _1 = 1,
    _2 = 2,
    _3 = 3,
}

export class GetDashboardDataOutput implements IGetDashboardDataOutput {
    totalTransSale: number | undefined;
    totalTransToday: number | undefined;
    totalTransCurrentSession: number | undefined;
    totalTransCurrentSessionSale: number | undefined;
    salesSummary: SalesSummaryData[] | undefined;
    transactionForToday: TransactionForToday[] | undefined;

    constructor(data?: IGetDashboardDataOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.totalTransSale = data["totalTransSale"];
            this.totalTransToday = data["totalTransToday"];
            this.totalTransCurrentSession = data["totalTransCurrentSession"];
            this.totalTransCurrentSessionSale = data["totalTransCurrentSessionSale"];
            this.salesSummary = data["salesSummary"];
            this.transactionForToday = data["transactionForToday"];
        }
    }

    static fromJS(data: any): GetDashboardDataOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetDashboardDataOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["totalTransSale"] = this.totalTransSale;
        data["totalTransToday"] = this.totalTransToday;
        data["totalTransCurrentSession"] = this.totalTransCurrentSession;
        data["totalTransCurrentSessionSale"] = this.totalTransCurrentSessionSale;
        data["salesSummary"] = this.salesSummary;
        data["transactionForToday"] = this.transactionForToday;
        return data;
    }
}

export interface IGetDashboardDataOutput {
    totalTransSale: number | undefined;
    totalTransToday: number | undefined;
    totalTransCurrentSession: number | undefined;
    totalTransCurrentSessionSale: number | undefined;
    salesSummary: SalesSummaryData[] | undefined;
    transactionForToday: TransactionForToday[] | undefined;
}


export class SalesSummaryData implements ISalesSummaryData {
    period: string | undefined;
    sales: number | undefined;
    trans: number | undefined;

    constructor(data?: ISalesSummaryData) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.period = data["period"];
            this.sales = data["sales"];
            this.trans = data["trans"];
        }
    }

    static fromJS(data: any): SalesSummaryData {
        data = typeof data === 'object' ? data : {};
        let result = new SalesSummaryData();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["period"] = this.period;
        data["sales"] = this.sales;
        data["trans"] = this.trans;
        return data;
    }
}

export interface ISalesSummaryData {
    period: string | undefined;
    sales: number | undefined;
    trans: number | undefined;
}


export class GetSalesSummaryOutput implements IGetSalesSummaryOutput {
    salesSummary!: SalesSummaryData[] | undefined;

    constructor(data?: IGetSalesSummaryOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["salesSummary"] && data["salesSummary"].constructor === Array) {
                this.salesSummary = [];
                for (let item of data["salesSummary"])
                    this.salesSummary.push(SalesSummaryData.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetSalesSummaryOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetSalesSummaryOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.salesSummary && this.salesSummary.constructor === Array) {
            data["salesSummary"] = [];
            for (let item of this.salesSummary)
                data["salesSummary"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetSalesSummaryOutput {
    salesSummary: SalesSummaryData[] | undefined;
}


//Session transaction stat data
export class GetSessionStatOutput implements IGetSessionStatOutput {
    sessionStat!: SessionStatData[] | undefined;

    constructor(data?: IGetSessionStatOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["sessionStat"] && data["sessionStat"].constructor === Array) {
                this.sessionStat = [];
                for (let item of data["sessionStat"])
                    this.sessionStat.push(SessionStatData.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetSessionStatOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetSessionStatOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.sessionStat && this.sessionStat.constructor === Array) {
            data["sessionStat"] = [];
            for (let item of this.sessionStat)
                data["sessionStat"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetSessionStatOutput {
    sessionStat: SessionStatData[] | undefined;
}

export class SessionStatData implements ISessionStatData {

    sessionName: string | undefined;
    totalTransaction: number | undefined;
    totalSale: number | undefined;

    constructor(data?: ISessionStatData) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.sessionName = data["sessionName"];
            this.totalTransaction = data["totalTransaction"];
            this.totalSale = data["totalSale"];
        }
    }

    static fromJS(data: any): SessionStatData {
        data = typeof data === 'object' ? data : {};
        let result = new SessionStatData();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["sessionName"] = this.sessionName;
        data["totalTransaction"] = this.totalTransaction;
        data["totalSale"] = this.totalSale;
        return data;
    }
}

export interface ISessionStatData {
    sessionName: string | undefined;
    totalTransaction: number | undefined;
    totalSale: number | undefined;
}


//Machine transaction stat data
export class GetMachineStatOutput implements IGetMachineStatOutput {
    machineStat!: MachineStatData[] | undefined;
    
    constructor(data?: IGetMachineStatOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["machineStat"] && data["machineStat"].constructor === Array) {
                this.machineStat = [];
                for (let item of data["machineStat"])
                    this.machineStat.push(MachineStatData.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetMachineStatOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetMachineStatOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.machineStat && this.machineStat.constructor === Array) {
            data["machineStat"] = [];
            for (let item of this.machineStat)
                data["machineStat"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetMachineStatOutput {
    machineStat: MachineStatData[] | undefined;
}

export class MachineStatData implements IMachineStatData {

    machineName: string | undefined;
    totalTransaction: number | undefined;
    totalSale: number | undefined;

    constructor(data?: IMachineStatData) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.machineName = data["machineName"];
            this.totalTransaction = data["totalTransaction"];
            this.totalSale = data["totalSale"];
        }
    }

    static fromJS(data: any): MachineStatData {
        data = typeof data === 'object' ? data : {};
        let result = new MachineStatData();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["machineName"] = this.machineName;
        data["totalTransaction"] = this.totalTransaction;
        data["totalSale"] = this.totalSale;
        return data;
    }
}

export interface IMachineStatData {
    machineName: string | undefined;
    totalTransaction: number | undefined;
    totalSale: number | undefined;
}

export class TransactionForToday implements ITransactionForToday {
    machineLogicalID: string | undefined;
    productName: Array<string> | undefined;
    locationCode: string | undefined;
    totalValue: number | undefined;
    dateTime: string | undefined;

    constructor(data?: ITransactionForToday) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.machineLogicalID = data["machineLogicalID"];
            this.productName = data["productName"];
            this.locationCode = data["locationCode"];
            this.totalValue = data["totalValue"];
            this.dateTime = data["dateTime"];
        }
    }

    static fromJS(data: any): Array<TransactionForToday> {
        data = typeof data === 'object' ? data : {};
        let result = new Array<TransactionForToday>();
        result = data;
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["machineLogicalID"] = this.machineLogicalID;
        data["productName"] = this.productName;
        data["locationCode"] = this.locationCode;
        data["totalValue"] = this.totalValue;
        data["dateTime"] = this.dateTime;
        return data;
    }
}

export interface ITransactionForToday {
    machineLogicalID: string | undefined;
    productName: Array<string> | undefined;
    locationCode: string | undefined;
    totalValue: number | undefined;
    dateTime: string | undefined;
}

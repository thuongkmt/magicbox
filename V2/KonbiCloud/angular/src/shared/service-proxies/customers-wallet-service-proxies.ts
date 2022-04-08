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
import { API_BASE_URL } from './service-proxies';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import * as moment from 'moment';

@Injectable()
export class CustomerKonbiWalletServiceServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @param filter (optional) 
     * @param customerFilter (optional) 
     * @param userNameFilter (optional) 
     * @param emailFilter (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getAll(filter: string | null | undefined, customerFilter: string | null | undefined, userNameFilter: string | null | undefined, emailFilter: string | null | undefined, sorting: string | null | undefined, skipCount: number | null | undefined, maxResultCount: number | null | undefined): Observable<PagedResultDtoOfCustomerWallet> {
        let url_ = this.baseUrl + "/api/services/app/CustomerKonbiWalletService/GetAll?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&"; 
        if (customerFilter !== undefined)
            url_ += "CustomerFilter=" + encodeURIComponent("" + customerFilter) + "&"; 
        if (userNameFilter !== undefined)
            url_ += "UserNameFilter=" + encodeURIComponent("" + userNameFilter) + "&"; 
        if (emailFilter !== undefined)
            url_ += "EmailFilter=" + encodeURIComponent("" + emailFilter) + "&"; 
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
                    return <Observable<PagedResultDtoOfCustomerWallet>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfCustomerWallet>><any>_observableThrow(response_);
        }));
    }

    protected processGetAll(response: HttpResponseBase): Observable<PagedResultDtoOfCustomerWallet> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? PagedResultDtoOfCustomerWallet.fromJS(resultData200) : new PagedResultDtoOfCustomerWallet();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfCustomerWallet>(<any>null);
    }

    /**
     * @param customerId (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getOrdersByCustomer(customerId: number | null | undefined, sorting: string | null | undefined, skipCount: number | null | undefined, maxResultCount: number | null | undefined): Observable<PagedResultDtoOfWalletTransaction> {
        let url_ = this.baseUrl + "/api/services/app/CustomerKonbiWalletService/GetOrdersByCustomer?";
        if (customerId !== undefined)
            url_ += "CustomerId=" + encodeURIComponent("" + customerId) + "&"; 
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
            return this.processGetOrdersByCustomer(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetOrdersByCustomer(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfWalletTransaction>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfWalletTransaction>><any>_observableThrow(response_);
        }));
    }

    protected processGetOrdersByCustomer(response: HttpResponseBase): Observable<PagedResultDtoOfWalletTransaction> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? PagedResultDtoOfWalletTransaction.fromJS(resultData200) : new PagedResultDtoOfWalletTransaction();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfWalletTransaction>(<any>null);
    }
}

export class PagedResultDtoOfCustomerWallet implements IPagedResultDtoOfCustomerWallet {
    totalCount!: number | undefined;
    items!: CustomerWallet[] | undefined;

    constructor(data?: IPagedResultDtoOfCustomerWallet) {
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
                    this.items.push(CustomerWallet.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfCustomerWallet {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfCustomerWallet();
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

export interface IPagedResultDtoOfCustomerWallet {
    totalCount: number | undefined;
    items: CustomerWallet[] | undefined;
}

export class CustomerWallet implements ICustomerWallet {
    customer!: string | undefined;
    total_spent!: number | undefined;
    orders_count!: number | undefined;
    is_paying_customer!: boolean | undefined;
    shipping!: CustomerShipping | undefined;
    billing!: CustomerBilling | undefined;
    password!: string | undefined;
    username!: string | undefined;
    avatar_url!: string | undefined;
    role!: string | undefined;
    first_name!: string | undefined;
    email!: string | undefined;
    date_modified_gmt!: moment.Moment | undefined;
    date_modified!: moment.Moment | undefined;
    date_created_gmt!: moment.Moment | undefined;
    date_created!: moment.Moment | undefined;
    id!: number | undefined;
    last_name!: string | undefined;
    meta_data!: CustomerMeta[] | undefined;
    total_topup!: number | undefined;
    wallet_balance!: number | undefined;
    sign_up!: moment.Moment | undefined;
    last_active!: moment.Moment | undefined;

    constructor(data?: ICustomerWallet) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.customer = data["customer"];
            this.total_spent = data["total_spent"];
            this.orders_count = data["orders_count"];
            this.is_paying_customer = data["is_paying_customer"];
            this.shipping = data["shipping"] ? CustomerShipping.fromJS(data["shipping"]) : <any>undefined;
            this.billing = data["billing"] ? CustomerBilling.fromJS(data["billing"]) : <any>undefined;
            this.password = data["password"];
            this.username = data["username"];
            this.avatar_url = data["avatar_url"];
            this.role = data["role"];
            this.first_name = data["first_name"];
            this.email = data["email"];
            this.date_modified_gmt = data["date_modified_gmt"] ? moment(data["date_modified_gmt"].toString()) : <any>undefined;
            this.date_modified = data["date_modified"] ? moment(data["date_modified"].toString()) : <any>undefined;
            this.date_created_gmt = data["date_created_gmt"] ? moment(data["date_created_gmt"].toString()) : <any>undefined;
            this.date_created = data["date_created"] ? moment(data["date_created"].toString()) : <any>undefined;
            this.id = data["id"];
            this.last_name = data["last_name"];
            if (data["meta_data"] && data["meta_data"].constructor === Array) {
                this.meta_data = [];
                for (let item of data["meta_data"])
                    this.meta_data.push(CustomerMeta.fromJS(item));
            }
            this.total_topup = data["total_topup"];
            this.wallet_balance = data["wallet_balance"];
            this.sign_up = data["sign_up"] ? moment(data["sign_up"].toString()) : <any>undefined;
            this.last_active = data["last_active"] ? moment(data["last_active"].toString()) : <any>undefined;
        }
    }

    static fromJS(data: any): CustomerWallet {
        data = typeof data === 'object' ? data : {};
        let result = new CustomerWallet();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["customer"] = this.customer;
        data["total_spent"] = this.total_spent;
        data["orders_count"] = this.orders_count;
        data["is_paying_customer"] = this.is_paying_customer;
        data["shipping"] = this.shipping ? this.shipping.toJSON() : <any>undefined;
        data["billing"] = this.billing ? this.billing.toJSON() : <any>undefined;
        data["password"] = this.password;
        data["username"] = this.username;
        data["avatar_url"] = this.avatar_url;
        data["role"] = this.role;
        data["first_name"] = this.first_name;
        data["email"] = this.email;
        data["date_modified_gmt"] = this.date_modified_gmt ? this.date_modified_gmt.toISOString() : <any>undefined;
        data["date_modified"] = this.date_modified ? this.date_modified.toISOString() : <any>undefined;
        data["date_created_gmt"] = this.date_created_gmt ? this.date_created_gmt.toISOString() : <any>undefined;
        data["date_created"] = this.date_created ? this.date_created.toISOString() : <any>undefined;
        data["id"] = this.id;
        data["last_name"] = this.last_name;
        if (this.meta_data && this.meta_data.constructor === Array) {
            data["meta_data"] = [];
            for (let item of this.meta_data)
                data["meta_data"].push(item.toJSON());
        }
        data["total_topup"] = this.total_topup;
        data["wallet_balance"] = this.wallet_balance;
        data["sign_up"] = this.sign_up ? this.sign_up.toISOString() : <any>undefined;
        data["last_active"] = this.last_active ? this.last_active.toISOString() : <any>undefined;
        return data; 
    }
}

export interface ICustomerWallet {
    customer: string | undefined;
    total_spent: number | undefined;
    orders_count: number | undefined;
    is_paying_customer: boolean | undefined;
    shipping: CustomerShipping | undefined;
    billing: CustomerBilling | undefined;
    password: string | undefined;
    username: string | undefined;
    avatar_url: string | undefined;
    role: string | undefined;
    first_name: string | undefined;
    email: string | undefined;
    date_modified_gmt: moment.Moment | undefined;
    date_modified: moment.Moment | undefined;
    date_created_gmt: moment.Moment | undefined;
    date_created: moment.Moment | undefined;
    id: number | undefined;
    last_name: string | undefined;
    meta_data: CustomerMeta[] | undefined;
    total_topup: number | undefined;
    wallet_balance: number | undefined;
    sign_up: moment.Moment | undefined;
    last_active: moment.Moment | undefined;
}

export class CustomerShipping implements ICustomerShipping {
    first_name!: string | undefined;
    last_name!: string | undefined;
    company!: string | undefined;
    address_1!: string | undefined;
    address_2!: string | undefined;
    city!: string | undefined;
    state!: string | undefined;
    postcode!: string | undefined;
    country!: string | undefined;

    constructor(data?: ICustomerShipping) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.first_name = data["first_name"];
            this.last_name = data["last_name"];
            this.company = data["company"];
            this.address_1 = data["address_1"];
            this.address_2 = data["address_2"];
            this.city = data["city"];
            this.state = data["state"];
            this.postcode = data["postcode"];
            this.country = data["country"];
        }
    }

    static fromJS(data: any): CustomerShipping {
        data = typeof data === 'object' ? data : {};
        let result = new CustomerShipping();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["first_name"] = this.first_name;
        data["last_name"] = this.last_name;
        data["company"] = this.company;
        data["address_1"] = this.address_1;
        data["address_2"] = this.address_2;
        data["city"] = this.city;
        data["state"] = this.state;
        data["postcode"] = this.postcode;
        data["country"] = this.country;
        return data; 
    }
}

export interface ICustomerShipping {
    first_name: string | undefined;
    last_name: string | undefined;
    company: string | undefined;
    address_1: string | undefined;
    address_2: string | undefined;
    city: string | undefined;
    state: string | undefined;
    postcode: string | undefined;
    country: string | undefined;
}

export class CustomerBilling implements ICustomerBilling {
    first_name!: string | undefined;
    last_name!: string | undefined;
    company!: string | undefined;
    address_1!: string | undefined;
    address_2!: string | undefined;
    city!: string | undefined;
    state!: string | undefined;
    postcode!: string | undefined;
    country!: string | undefined;
    email!: string | undefined;
    phone!: string | undefined;

    constructor(data?: ICustomerBilling) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.first_name = data["first_name"];
            this.last_name = data["last_name"];
            this.company = data["company"];
            this.address_1 = data["address_1"];
            this.address_2 = data["address_2"];
            this.city = data["city"];
            this.state = data["state"];
            this.postcode = data["postcode"];
            this.country = data["country"];
            this.email = data["email"];
            this.phone = data["phone"];
        }
    }

    static fromJS(data: any): CustomerBilling {
        data = typeof data === 'object' ? data : {};
        let result = new CustomerBilling();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["first_name"] = this.first_name;
        data["last_name"] = this.last_name;
        data["company"] = this.company;
        data["address_1"] = this.address_1;
        data["address_2"] = this.address_2;
        data["city"] = this.city;
        data["state"] = this.state;
        data["postcode"] = this.postcode;
        data["country"] = this.country;
        data["email"] = this.email;
        data["phone"] = this.phone;
        return data; 
    }
}

export interface ICustomerBilling {
    first_name: string | undefined;
    last_name: string | undefined;
    company: string | undefined;
    address_1: string | undefined;
    address_2: string | undefined;
    city: string | undefined;
    state: string | undefined;
    postcode: string | undefined;
    country: string | undefined;
    email: string | undefined;
    phone: string | undefined;
}

export class CustomerMeta implements ICustomerMeta {
    id!: number | undefined;
    key!: string | undefined;
    value!: any | undefined;

    constructor(data?: ICustomerMeta) {
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
            this.key = data["key"];
            this.value = data["value"];
        }
    }

    static fromJS(data: any): CustomerMeta {
        data = typeof data === 'object' ? data : {};
        let result = new CustomerMeta();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["key"] = this.key;
        data["value"] = this.value;
        return data; 
    }
}

export interface ICustomerMeta {
    id: number | undefined;
    key: string | undefined;
    value: any | undefined;
}

export class PagedResultDtoOfWalletTransaction implements IPagedResultDtoOfWalletTransaction {
    totalCount!: number | undefined;
    items!: WalletTransaction[] | undefined;

    constructor(data?: IPagedResultDtoOfWalletTransaction) {
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
                    this.items.push(WalletTransaction.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfWalletTransaction {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfWalletTransaction();
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

export interface IPagedResultDtoOfWalletTransaction {
    totalCount: number | undefined;
    items: WalletTransaction[] | undefined;
}

export class WalletTransaction implements IWalletTransaction {
    transaction_id!: number | undefined;
    blog_id!: number | undefined;
    user_id!: number | undefined;
    type!: string | undefined;
    amount!: number | undefined;
    balance!: number | undefined;
    currency!: string | undefined;
    details!: string | undefined;
    deleted!: string | undefined;
    date!: moment.Moment | undefined;
    user_name!: string | undefined;
    created_by_name!: string | undefined;

    constructor(data?: IWalletTransaction) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.transaction_id = data["transaction_id"];
            this.blog_id = data["blog_id"];
            this.user_id = data["user_id"];
            this.type = data["type"];
            this.amount = data["amount"];
            this.balance = data["balance"];
            this.currency = data["currency"];
            this.details = data["details"];
            this.deleted = data["deleted"];
            this.date = data["date"] ? moment(data["date"].toString()) : <any>undefined;
            this.user_name = data["user_name"];
            this.created_by_name = data["created_by_name"];
        }
    }

    static fromJS(data: any): WalletTransaction {
        data = typeof data === 'object' ? data : {};
        let result = new WalletTransaction();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["transaction_id"] = this.transaction_id;
        data["blog_id"] = this.blog_id;
        data["user_id"] = this.user_id;
        data["type"] = this.type;
        data["amount"] = this.amount;
        data["balance"] = this.balance;
        data["currency"] = this.currency;
        data["details"] = this.details;
        data["deleted"] = this.deleted;
        data["date"] = this.date ? this.date.toISOString() : <any>undefined;
        data["user_name"] = this.user_name;
        data["created_by_name"] = this.created_by_name;
        return data; 
    }
}

export interface IWalletTransaction {
    transaction_id: number | undefined;
    blog_id: number | undefined;
    user_id: number | undefined;
    type: string | undefined;
    amount: number | undefined;
    balance: number | undefined;
    currency: string | undefined;
    details: string | undefined;
    deleted: string | undefined;
    date: moment.Moment | undefined;
    user_name: string | undefined;
    created_by_name: string | undefined;
}

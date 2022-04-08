// import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
// import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
// import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
// import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
// import * as moment from 'moment';
// import { API_BASE_URL } from './service-proxies';
// import { blobToText, throwException } from './service-base';

// @Injectable()
// export class InventoryServiceProxy {
//     private http: HttpClient;
//     private baseUrl: string;
//     protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

//     constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
//         this.http = http;
//         this.baseUrl = baseUrl ? baseUrl : "";
//     }

//     /**
//      * @return Success
//      */
//     getInventoryOverview(): Observable<ListResultDtoOfInventoryOverviewDto> {
//         let url_ = this.baseUrl + "/api/services/app/Inventories/GetInventoryOverview";
//         url_ = url_.replace(/[?&]$/, "");

//         let options_: any = {
//             observe: "response",
//             responseType: "blob",
//             headers: new HttpHeaders({
//                 "Accept": "application/json"
//             })
//         };

//         return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
//             return this.processGetInventoryOverview(response_);
//         })).pipe(_observableCatch((response_: any) => {
//             if (response_ instanceof HttpResponseBase) {
//                 try {
//                     return this.processGetInventoryOverview(<any>response_);
//                 } catch (e) {
//                     return <Observable<ListResultDtoOfInventoryOverviewDto>><any>_observableThrow(e);
//                 }
//             } else
//                 return <Observable<ListResultDtoOfInventoryOverviewDto>><any>_observableThrow(response_);
//         }));
//     }

//     /**
//      * @return Success
//      */
//     getInventoryRealTimeOverview(): Observable<ListResultDtoOfInventoryOverviewDto> {
//         let url_ = this.baseUrl + "/api/services/app/Inventories/GetMachinesInventoryRealTime";
//         url_ = url_.replace(/[?&]$/, "");

//         let options_: any = {
//             observe: "response",
//             responseType: "blob",
//             headers: new HttpHeaders({
//                 "Accept": "application/json"
//             })
//         };

//         return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
//             return this.processGetInventoryOverview(response_);
//         })).pipe(_observableCatch((response_: any) => {
//             if (response_ instanceof HttpResponseBase) {
//                 try {
//                     return this.processGetInventoryOverview(<any>response_);
//                 } catch (e) {
//                     return <Observable<ListResultDtoOfInventoryOverviewDto>><any>_observableThrow(e);
//                 }
//             } else
//                 return <Observable<ListResultDtoOfInventoryOverviewDto>><any>_observableThrow(response_);
//         }));
//     }

//     protected processGetInventoryOverview(response: HttpResponseBase): Observable<ListResultDtoOfInventoryOverviewDto> {
//         const status = response.status;
//         const responseBlob =
//             response instanceof HttpResponse ? response.body :
//                 (<any>response).error instanceof Blob ? (<any>response).error : undefined;

//         let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
//         if (status === 200) {
//             return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
//                 let result200: any = null;
//                 let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
//                 result200 = resultData200 ? ListResultDtoOfInventoryOverviewDto.fromJS(resultData200) : new ListResultDtoOfInventoryOverviewDto();
//                 return _observableOf(result200);
//             }));
//         } else if (status !== 200 && status !== 204) {
//             return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
//                 return throwException("An unexpected server error occurred.", status, _responseText, _headers);
//             }));
//         }
//         return _observableOf<ListResultDtoOfInventoryOverviewDto>(<any>null);
//     }

//     /**
//      * @param machineId (optional) 
//      * @param topupId (optional) 
//      * @return Success
//      */
//     getInventoryDetail(machineId: string | null | undefined, topupId: string | null | undefined): Observable<InventoryDetailOutput> {
//         let url_ = this.baseUrl + "/api/services/app/Inventories/GetInventoryDetail?";
//         if (machineId !== undefined)
//             url_ += "machineId=" + encodeURIComponent("" + machineId) + "&";
//         if (topupId !== undefined)
//             url_ += "topupId=" + encodeURIComponent("" + topupId) + "&";
//         url_ = url_.replace(/[?&]$/, "");

//         let options_: any = {
//             observe: "response",
//             responseType: "blob",
//             headers: new HttpHeaders({
//                 "Accept": "application/json"
//             })
//         };

//         return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
//             return this.processGetInventoryDetail(response_);
//         })).pipe(_observableCatch((response_: any) => {
//             if (response_ instanceof HttpResponseBase) {
//                 try {
//                     return this.processGetInventoryDetail(<any>response_);
//                 } catch (e) {
//                     return <Observable<InventoryDetailOutput>><any>_observableThrow(e);
//                 }
//             } else
//                 return <Observable<InventoryDetailOutput>><any>_observableThrow(response_);
//         }));
//     }

//     getInventoryDetailRealTime(machineId: string | null | undefined, topupId: string | null | undefined): Observable<InventoryDetailOutput> {
//         let url_ = this.baseUrl + "/api/services/app/Inventories/GetMachineInventoryDetailRealTime?";
//         if (machineId !== undefined)
//             url_ += "machineId=" + encodeURIComponent("" + machineId) + "&";
//         if (topupId !== undefined)
//             url_ += "topupId=" + encodeURIComponent("" + topupId) + "&";
//         url_ = url_.replace(/[?&]$/, "");

//         let options_: any = {
//             observe: "response",
//             responseType: "blob",
//             headers: new HttpHeaders({
//                 "Accept": "application/json"
//             })
//         };

//         return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
//             return this.processGetInventoryDetail(response_);
//         })).pipe(_observableCatch((response_: any) => {
//             if (response_ instanceof HttpResponseBase) {
//                 try {
//                     return this.processGetInventoryDetail(<any>response_);
//                 } catch (e) {
//                     return <Observable<InventoryDetailOutput>><any>_observableThrow(e);
//                 }
//             } else
//                 return <Observable<InventoryDetailOutput>><any>_observableThrow(response_);
//         }));
//     }

//     protected processGetInventoryDetail(response: HttpResponseBase): Observable<InventoryDetailOutput> {
//         const status = response.status;
//         const responseBlob =
//             response instanceof HttpResponse ? response.body :
//                 (<any>response).error instanceof Blob ? (<any>response).error : undefined;

//         let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
//         if (status === 200) {
//             return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
//                 let result200: any = null;
//                 let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
//                 result200 = resultData200 ? InventoryDetailOutput.fromJS(resultData200) : new InventoryDetailOutput();
//                 return _observableOf(result200);
//             }));
//         } else if (status !== 200 && status !== 204) {
//             return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
//                 return throwException("An unexpected server error occurred.", status, _responseText, _headers);
//             }));
//         }
//         return _observableOf<InventoryDetailOutput>(<any>null);
//     }

//     getCurrentInventories(input: GetCurrentInventoryInput): Observable<PageResultDtoOfCurrentInventoryDto> {
//         let url_ = this.baseUrl + "/api/services/app/Inventories/CurrentInventories";
//         // if (filter !== undefined)
//         //     url_ += "Filter=" + encodeURIComponent("" + filter) + "&"; 

//         // if (tagIdFilter !== undefined)
//         //     url_ += "TagIdFilter=" + encodeURIComponent("" + tagIdFilter) + "&"; 

//         // if (machinesFilter !== undefined && machinesFilter.length > 0)
//         //     url_ += "MachinesFilter=" + encodeURIComponent("" + machinesFilter) + "&";

//         // if (sorting !== undefined)
//         //     url_ += "Sorting=" + encodeURIComponent("" + sorting) + "&";

//         // if (maxResultCount !== undefined)
//         //     url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&";

//         // if (skipCount !== undefined)
//         //     url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&";

//         // url_ = url_.replace(/[?&]$/, "");

//         let options_ : any = {
//             body: JSON.stringify(input),
//             observe: "response",
//             responseType: "blob",
//             headers: new HttpHeaders({
//                 "Content-Type": "application/json",
//                 "Accept": "application/json"
//             })
//         };

//         return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_ : any) => {
//             return this.processGetCurrentInventories(response_);
//         })).pipe(_observableCatch((response_: any) => {
//             if (response_ instanceof HttpResponseBase) {
//                 try {
//                     return this.processGetCurrentInventories(<any>response_);
//                 } catch (e) {
//                     return <Observable<PageResultDtoOfCurrentInventoryDto>><any>_observableThrow(e);
//                 }
//             } else
//                 return <Observable<PageResultDtoOfCurrentInventoryDto>><any>_observableThrow(response_);
//         }));
//     }

//     protected processGetCurrentInventories(response: HttpResponseBase): Observable<PageResultDtoOfCurrentInventoryDto> {
//         const status = response.status;
//         const responseBlob = 
//             response instanceof HttpResponse ? response.body : 
//             (<any>response).error instanceof Blob ? (<any>response).error : undefined;

//         let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
//         if (status === 200) {
//             return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
//             let result200: any = null;
//             let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
//             result200 = resultData200 ? PageResultDtoOfCurrentInventoryDto.fromJS(resultData200) : new PageResultDtoOfCurrentInventoryDto();
//             return _observableOf(result200);
//             }));
//         } else if (status !== 200 && status !== 204) {
//             return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
//             return throwException("An unexpected server error occurred.", status, _responseText, _headers);
//             }));
//         }
//         return _observableOf<PageResultDtoOfCurrentInventoryDto>(<any>null);
//     }

    
//     /**
//      * @return Success
//      */
//     getInventoryForReport(): Observable<InventoryDetailForRepportOutput[]> {
//         let url_ = this.baseUrl + "/api/services/app/Inventories/GetInventoryForReport";
//         url_ = url_.replace(/[?&]$/, "");

//         let options_ : any = {
//             observe: "response",
//             responseType: "blob",
//             headers: new HttpHeaders({
//                 "Accept": "application/json"
//             })
//         };

//         return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_ : any) => {
//             return this.processGetInventoryForReport(response_);
//         })).pipe(_observableCatch((response_: any) => {
//             if (response_ instanceof HttpResponseBase) {
//                 try {
//                     return this.processGetInventoryForReport(<any>response_);
//                 } catch (e) {
//                     return <Observable<InventoryDetailForRepportOutput[]>><any>_observableThrow(e);
//                 }
//             } else
//                 return <Observable<InventoryDetailForRepportOutput[]>><any>_observableThrow(response_);
//         }));
//     }

//     protected processGetInventoryForReport(response: HttpResponseBase): Observable<InventoryDetailForRepportOutput[]> {
//         const status = response.status;
//         const responseBlob = 
//             response instanceof HttpResponse ? response.body : 
//             (<any>response).error instanceof Blob ? (<any>response).error : undefined;

//         let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
//         if (status === 200) {
//             return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
//             let result200: any = null;
//             let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
//             if (resultData200 && resultData200.constructor === Array) {
//                 result200 = [];
//                 for (let item of resultData200)
//                     result200.push(InventoryDetailForRepportOutput.fromJS(item));
//             }
//             return _observableOf(result200);
//             }));
//         } else if (status !== 200 && status !== 204) {
//             return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
//             return throwException("An unexpected server error occurred.", status, _responseText, _headers);
//             }));
//         }
//         return _observableOf<InventoryDetailForRepportOutput[]>(<any>null);
//     }

// }

// export class InventoryDetailForRepportOutput implements IInventoryDetailForRepportOutput {
//     machineName!: string | undefined;
//     topupDate!: moment.Moment | undefined;
//     total!: number | undefined;
//     currentStock!: number | undefined;
//     leftOver!: number | undefined;
//     sold!: number | undefined;
//     error!: number | undefined;
//     inventoryDetailList!: InventoryDetailForReportDto[] | undefined;

//     constructor(data?: IInventoryDetailForRepportOutput) {
//         if (data) {
//             for (var property in data) {
//                 if (data.hasOwnProperty(property))
//                     (<any>this)[property] = (<any>data)[property];
//             }
//         }
//     }

//     init(data?: any) {
//         if (data) {
//             this.machineName = data["machineName"];
//             this.topupDate = data["topupDate"] ? moment(data["topupDate"].toString()) : <any>undefined;
//             this.total = data["total"];
//             this.currentStock = data["currentStock"];
//             this.leftOver = data["leftOver"];
//             this.sold = data["sold"];
//             this.error = data["error"];
//             if (data["inventoryDetailList"] && data["inventoryDetailList"].constructor === Array) {
//                 this.inventoryDetailList = [];
//                 for (let item of data["inventoryDetailList"])
//                     this.inventoryDetailList.push(InventoryDetailForReportDto.fromJS(item));
//             }
//         }
//     }

//     static fromJS(data: any): InventoryDetailForRepportOutput {
//         data = typeof data === 'object' ? data : {};
//         let result = new InventoryDetailForRepportOutput();
//         result.init(data);
//         return result;
//     }

//     toJSON(data?: any) {
//         data = typeof data === 'object' ? data : {};
//         data["machineName"] = this.machineName;
//         data["topupDate"] = this.topupDate ? this.topupDate.toISOString() : <any>undefined;
//         data["total"] = this.total;
//         data["currentStock"] = this.currentStock;
//         data["leftOver"] = this.leftOver;
//         data["sold"] = this.sold;
//         data["error"] = this.error;
//         if (this.inventoryDetailList && this.inventoryDetailList.constructor === Array) {
//             data["inventoryDetailList"] = [];
//             for (let item of this.inventoryDetailList)
//                 data["inventoryDetailList"].push(item.toJSON());
//         }
//         return data; 
//     }
// }

// export interface IInventoryDetailForRepportOutput {
//     machineName: string | undefined;
//     topupDate: moment.Moment | undefined;
//     total: number | undefined;
//     currentStock: number | undefined;
//     leftOver: number | undefined;
//     sold: number | undefined;
//     error: number | undefined;
//     inventoryDetailList: InventoryDetailForReportDto[] | undefined;
// }

// export class InventoryDetailForReportDto implements IInventoryDetailForReportDto {
//     lastSoldDate!: moment.Moment | undefined;
//     productName!: string | undefined;
//     productCategory!: string | undefined;
//     productSku!: string | undefined;
//     productExpiredDate!: string | undefined;
//     productDescription!: string | undefined;
//     productPrice!: number | undefined;
//     currentStock!: number | undefined;
//     total!: number | undefined;
//     sold!: number | undefined;
//     error!: number | undefined;
//     leftOver!: number | undefined;

//     constructor(data?: IInventoryDetailForReportDto) {
//         if (data) {
//             for (var property in data) {
//                 if (data.hasOwnProperty(property))
//                     (<any>this)[property] = (<any>data)[property];
//             }
//         }
//     }

//     init(data?: any) {
//         if (data) {
//             this.lastSoldDate = data["lastSoldDate"] ? moment(data["lastSoldDate"].toString()) : <any>undefined;
//             this.productName = data["productName"];
//             this.productCategory = data["productCategory"];
//             this.productSku = data["productSku"];
//             this.productExpiredDate = data["productExpiredDate"];
//             this.productDescription = data["productDescription"];
//             this.productPrice = data["productPrice"];
//             this.currentStock = data["currentStock"];
//             this.total = data["total"];
//             this.sold = data["sold"];
//             this.error = data["error"];
//             this.leftOver = data["leftOver"];
//         }
//     }

//     static fromJS(data: any): InventoryDetailForReportDto {
//         data = typeof data === 'object' ? data : {};
//         let result = new InventoryDetailForReportDto();
//         result.init(data);
//         return result;
//     }

//     toJSON(data?: any) {
//         data = typeof data === 'object' ? data : {};
//         data["lastSoldDate"] = this.lastSoldDate ? this.lastSoldDate.toISOString() : <any>undefined;
//         data["productName"] = this.productName;
//         data["productCategory"] = this.productCategory;
//         data["productSku"] = this.productSku;
//         data["productExpiredDate"] = this.productExpiredDate;
//         data["productDescription"] = this.productDescription;
//         data["productPrice"] = this.productPrice;
//         data["currentStock"] = this.currentStock;
//         data["total"] = this.total;
//         data["sold"] = this.sold;
//         data["error"] = this.error;
//         data["leftOver"] = this.leftOver;
//         return data; 
//     }
// }

// export interface IInventoryDetailForReportDto {
//     lastSoldDate: moment.Moment | undefined;
//     productName: string | undefined;
//     productCategory: string | undefined;
//     productSku: string | undefined;
//     productExpiredDate: string | undefined;
//     productDescription: string | undefined;
//     productPrice: number | undefined;
//     currentStock: number | undefined;
//     total: number | undefined;
//     sold: number | undefined;
//     error: number | undefined;
//     leftOver: number | undefined;
// }
// export class ListResultDtoOfInventoryOverviewDto implements IListResultDtoOfInventoryOverviewDto {
//     items!: InventoryOverviewDto[] | undefined;

//     constructor(data?: IListResultDtoOfInventoryOverviewDto) {
//         if (data) {
//             for (var property in data) {
//                 if (data.hasOwnProperty(property))
//                     (<any>this)[property] = (<any>data)[property];
//             }
//         }
//     }

//     init(data?: any) {
//         if (data) {
//             if (data["items"] && data["items"].constructor === Array) {
//                 this.items = [];
//                 for (let item of data["items"])
//                     this.items.push(InventoryOverviewDto.fromJS(item));
//             }
//         }
//     }

//     static fromJS(data: any): ListResultDtoOfInventoryOverviewDto {
//         data = typeof data === 'object' ? data : {};
//         let result = new ListResultDtoOfInventoryOverviewDto();
//         result.init(data);
//         return result;
//     }

//     toJSON(data?: any) {
//         data = typeof data === 'object' ? data : {};
//         if (this.items && this.items.constructor === Array) {
//             data["items"] = [];
//             for (let item of this.items)
//                 data["items"].push(item.toJSON());
//         }
//         return data;
//     }
// }

// export interface IListResultDtoOfInventoryOverviewDto {
//     items: InventoryOverviewDto[] | undefined;
// }

// export class InventoryOverviewDto implements IInventoryOverviewDto {
//     machineId!: string | undefined;
//     topupId!: string | undefined;
//     machineName!: string | undefined;
//     topupDate!: moment.Moment | undefined;
//     leftOver!: number | undefined;
//     sold!: number | undefined;
//     error!: number | undefined;
//     currentStock!: number | undefined;

//     constructor(data?: IInventoryOverviewDto) {
//         if (data) {
//             for (var property in data) {
//                 if (data.hasOwnProperty(property))
//                     (<any>this)[property] = (<any>data)[property];
//             }
//         }
//     }

//     init(data?: any) {
//         if (data) {
//             this.machineId = data["machineId"];
//             this.topupId = data["topupId"];
//             this.machineName = data["machineName"];
//             this.topupDate = data["topupDate"] ? moment(data["topupDate"].toString()) : <any>undefined;
//             this.leftOver = data["leftOver"];
//             this.sold = data["sold"];
//             this.error = data["error"];
//             this.currentStock = data["currentStock"];
//         }
//     }

//     static fromJS(data: any): InventoryOverviewDto {
//         data = typeof data === 'object' ? data : {};
//         let result = new InventoryOverviewDto();
//         result.init(data);
//         return result;
//     }

//     toJSON(data?: any) {
//         data = typeof data === 'object' ? data : {};
//         data["machineId"] = this.machineId;
//         data["topupId"] = this.topupId;
//         data["machineName"] = this.machineName;
//         data["topupDate"] = this.topupDate ? this.topupDate.toISOString() : <any>undefined;
//         data["leftOver"] = this.leftOver;
//         data["sold"] = this.sold;
//         data["error"] = this.error;
//         data["currentStock"] = this.currentStock;
//         return data;
//     }
// }

// export interface IInventoryOverviewDto {
//     machineId: string | undefined;
//     topupId: string | undefined;
//     machineName: string | undefined;
//     topupDate: moment.Moment | undefined;
//     leftOver: number | undefined;
//     sold: number | undefined;
//     error: number | undefined;
//     currentStock: number | undefined;
// }

// export class InventoryDetailOutput implements IInventoryDetailOutput {
//     machineName!: string | undefined;
//     topupDate!: moment.Moment | undefined;
//     total!: number | undefined;
//     leftOver!: number | undefined;
//     sold!: number | undefined;
//     error!: number | undefined;
//     inventoryDetailList!: InventoryDetailDto[] | undefined;
//     currentStock!: number | undefined;

//     constructor(data?: IInventoryDetailOutput) {
//         if (data) {
//             for (var property in data) {
//                 if (data.hasOwnProperty(property))
//                     (<any>this)[property] = (<any>data)[property];
//             }
//         }
//     }

//     init(data?: any) {
//         if (data) {
//             this.machineName = data["machineName"];
//             this.topupDate = data["topupDate"] ? moment(data["topupDate"].toString()) : <any>undefined;
//             this.total = data["total"];
//             this.leftOver = data["leftOver"];
//             this.sold = data["sold"];
//             this.error = data["error"];
//             this.currentStock = data["currentStock"];
//             if (data["inventoryDetailList"] && data["inventoryDetailList"].constructor === Array) {
//                 this.inventoryDetailList = [];
//                 for (let item of data["inventoryDetailList"])
//                     this.inventoryDetailList.push(InventoryDetailDto.fromJS(item));
//             }
//         }
//     }

//     static fromJS(data: any): InventoryDetailOutput {
//         data = typeof data === 'object' ? data : {};
//         let result = new InventoryDetailOutput();
//         result.init(data);
//         return result;
//     }

//     toJSON(data?: any) {
//         data = typeof data === 'object' ? data : {};
//         data["machineName"] = this.machineName;
//         data["topupDate"] = this.topupDate ? this.topupDate.toISOString() : <any>undefined;
//         data["total"] = this.total;
//         data["leftOver"] = this.leftOver;
//         data["sold"] = this.sold;
//         data["error"] = this.error;
//         data["currentStock"] = this.currentStock;

//         if (this.inventoryDetailList && this.inventoryDetailList.constructor === Array) {
//             data["inventoryDetailList"] = [];
//             for (let item of this.inventoryDetailList)
//                 data["inventoryDetailList"].push(item.toJSON());
//         }
//         return data;
//     }
// }

// export interface IInventoryDetailOutput {
//     machineName: string | undefined;
//     topupDate: moment.Moment | undefined;
//     total: number | undefined;
//     leftOver: number | undefined;
//     sold: number | undefined;
//     error: number | undefined;
//     currentStock: number | undefined;
//     inventoryDetailList: InventoryDetailDto[] | undefined;
// }

// export class InventoryDetailDto implements IInventoryDetailDto {
//     lastSoldDate!: moment.Moment | undefined;
//     productName!: string | undefined;
//     total!: number | undefined;
//     sold!: number | undefined;
//     error!: number | undefined;
//     leftOver!: number | undefined;
//     currentStock!: number | undefined;

//     constructor(data?: IInventoryDetailDto) {
//         if (data) {
//             for (var property in data) {
//                 if (data.hasOwnProperty(property))
//                     (<any>this)[property] = (<any>data)[property];
//             }
//         }
//     }

//     init(data?: any) {
//         if (data) {
//             this.lastSoldDate = data["lastSoldDate"] ? moment(data["lastSoldDate"].toString()) : <any>undefined;
//             this.productName = data["productName"];
//             this.total = data["total"];
//             this.sold = data["sold"];
//             this.error = data["error"];
//             this.leftOver = data["leftOver"];
//             this.currentStock = data["currentStock"];
//         }
//     }

//     static fromJS(data: any): InventoryDetailDto {
//         data = typeof data === 'object' ? data : {};
//         let result = new InventoryDetailDto();
//         result.init(data);
//         return result;
//     }

//     toJSON(data?: any) {
//         data = typeof data === 'object' ? data : {};
//         data["lastSoldDate"] = this.lastSoldDate ? this.lastSoldDate.toISOString() : <any>undefined;
//         data["productName"] = this.productName;
//         data["total"] = this.total;
//         data["sold"] = this.sold;
//         data["error"] = this.error;
//         data["leftOver"] = this.leftOver;
//         data["currentStock"] = this.currentStock;
//         return data;
//     }
// }

// export interface IInventoryDetailDto {
//     lastSoldDate: moment.Moment | undefined;
//     productName: string | undefined;
//     total: number | undefined;
//     sold: number | undefined;
//     error: number | undefined;
//     leftOver: number | undefined;
//     currentStock: number | undefined;
// }

// export class PageResultDtoOfCurrentInventoryDto implements IPageResultDtoOfCurrentInventoryDto {
//     totalCount: number | undefined;
//     items!: CurrentInventoryDto[] | undefined;

//     constructor(data?: IPageResultDtoOfCurrentInventoryDto) {
//         if (data) {
//             for (var property in data) {
//                 if (data.hasOwnProperty(property))
//                     (<any>this)[property] = (<any>data)[property];
//             }
//         }
//     }

//     init(data?: any) {
//         if (data) {
//             this.totalCount = data["totalCount"];
//             if (data["items"] && data["items"].constructor === Array) {
//                 this.items = [];
//                 for (let item of data["items"])
//                     this.items.push(CurrentInventoryDto.fromJS(item));
//             }
//         }
//     }

//     static fromJS(data: any): PageResultDtoOfCurrentInventoryDto {
//         data = typeof data === 'object' ? data : {};
//         let result = new PageResultDtoOfCurrentInventoryDto();
//         result.init(data);
//         return result;
//     }

//     toJSON(data?: any) {
//         data = typeof data === 'object' ? data : {};
//         data["totalCount"] = this.totalCount;
//         if (this.items && this.items.constructor === Array) {
//             data["items"] = [];
//             for (let item of this.items)
//                 data["items"].push(item.toJSON());
//         }
//         return data; 
//     }
// }

// export interface IPageResultDtoOfCurrentInventoryDto {
//     totalCount: number | undefined;
//     items: CurrentInventoryDto[] | undefined;
// }

// export class CurrentInventoryDto implements ICurrentInventoryDto {
//     name!: string | undefined;
//     tagId!: string | undefined;
//     machineName!: string | undefined;

//     constructor(data?: ICurrentInventoryDto) {
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
//             this.tagId = data["tagId"];
//             this.machineName = data["machineName"];
//         }
//     }

//     static fromJS(data: any): CurrentInventoryDto {
//         data = typeof data === 'object' ? data : {};
//         let result = new CurrentInventoryDto();
//         result.init(data);
//         return result;
//     }

//     toJSON(data?: any) {
//         data = typeof data === 'object' ? data : {};
//         data["name"] = this.name;
//         data["tagId"] = this.tagId;
//         data["machineName"] = this.machineName;
//         return data; 
//     }
// }

// export interface ICurrentInventoryDto {
//     name: string | undefined;
//     tagId: string | undefined;
//     machineName: string | undefined;
// }

// export class GetCurrentInventoryInput {
//     Filter: string;
//     TagIdFilter: string;
//     MachinesFilter: any[];
//     Sorting: string;
//     MaxResultCount: number;
//     SkipCount: number;
// }

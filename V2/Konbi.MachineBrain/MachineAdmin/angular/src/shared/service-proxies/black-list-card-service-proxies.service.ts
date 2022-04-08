import { Injectable, Inject, Optional } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpResponseBase, HttpResponse, HttpHeaders, HttpClient } from '@angular/common/http';
import * as ApiServiceProxies from './service-proxies';
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { throwError as _observableThrow, of as _observableOf } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

@Injectable()
export class BlackListCardServiceProxiesService {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(ApiServiceProxies.API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    getAll(
      skipCount: number | null | undefined,
      maxResultCount: number | null | undefined,
      sorting: string | null | undefined,
    ): Observable<PagedResultDtoOfBlackListCardDto> {
      let url_ = this.baseUrl + "/api/services/app/BlackListCards/GetAll?";
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
          return this.processGetAll(response_);
      }).catch((response_: any) => {
          if (response_ instanceof HttpResponseBase) {
              try {
                  return this.processGetAll(<any>response_);
              } catch (e) {
                  return <Observable<PagedResultDtoOfBlackListCardDto>><any>Observable.throw(e);
              }
          } else
              return <Observable<PagedResultDtoOfBlackListCardDto>><any>Observable.throw(response_);
      });
    }

    protected processGetAll(response: HttpResponseBase): Observable<PagedResultDtoOfBlackListCardDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PagedResultDtoOfBlackListCardDto.fromJS(resultData200) : new PagedResultDtoOfBlackListCardDto();
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
        return Observable.of<PagedResultDtoOfBlackListCardDto>(<any>null);
    }

    delete(id: string | null | undefined): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/BlackListCards/Delete?";
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

    createOrEdit(input: BlackListCardDto | null | undefined): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/BlackListCards/Save";
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

    getBlackListCardForEdit(id: string | null | undefined): Observable<BlackListCardDto> {
        let url_ = this.baseUrl + "/api/services/app/BlackListCards/GetBlackListCardForEdit?";
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
            return this.processGetBlackListCardForEdit(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetBlackListCardForEdit(<any>response_);
                } catch (e) {
                    return <Observable<BlackListCardDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<BlackListCardDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetBlackListCardForEdit(response: HttpResponseBase): Observable<BlackListCardDto> {
        const status = response.status;
        const responseBlob = 
            response instanceof HttpResponse ? response.body : 
            (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); }};
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            let result200: any = null;
            let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
            result200 = resultData200 ? BlackListCardDto.fromJS(resultData200) : new BlackListCardDto();
            return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
            return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<BlackListCardDto>(<any>null);
    }
}


export class BlackListCardDto {
    id: string | undefined;
    cardLabel: string;
    cardNumber: string;
    unpaidAmount: number;

    constructor(data?: BlackListCardDto) {
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
            this.cardLabel = data["cardLabel"];
            this.cardNumber = data["cardNumber"];
            this.unpaidAmount = data["unpaidAmount"];
        }
    }

    static fromJS(data: any): BlackListCardDto {
        data = typeof data === 'object' ? data : {};
        let result = new BlackListCardDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["unpaidAmount"] = this.unpaidAmount;
        data["cardLabel"] = this.cardLabel;
        data["cardNumber"] = this.cardNumber;

        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new BlackListCardDto();
        result.init(json);
        return result;
    }
}

export interface IPagedResultDtoOfBlackListCardDto {
  totalCount: number | undefined;
  items: BlackListCardDto[] | undefined;
}

export class PagedResultDtoOfBlackListCardDto implements IPagedResultDtoOfBlackListCardDto {
  totalCount: number | undefined;
  items: BlackListCardDto[] | undefined;

  constructor(data?: IPagedResultDtoOfBlackListCardDto) {
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
                  this.items.push(BlackListCardDto.fromJS(item));
          }
      }
  }

  static fromJS(data: any): PagedResultDtoOfBlackListCardDto {
      data = typeof data === 'object' ? data : {};
      let result = new PagedResultDtoOfBlackListCardDto();
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
      let result = new PagedResultDtoOfBlackListCardDto();
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


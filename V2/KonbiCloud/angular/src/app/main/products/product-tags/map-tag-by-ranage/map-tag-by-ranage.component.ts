import { Component, ViewChild, Injector, Output, EventEmitter, Inject, Optional, } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { API_BASE_URL, ListProductTagDto, ProductTagInputDto, ProductTagsServiceProxy } from '@shared/service-proxies/service-proxies';
import { SelectionCategory, CategoryServiceProxy, PagedResultDtoOfGetProductCategoryForViewDto, CategoryDto } from '@shared/service-proxies/category-service-proxies';
import { ProductsServiceProxy } from '@shared/service-proxies/product-service-proxies';

import { AppComponentBase } from '@shared/common/app-component-base';
import * as moment from 'moment';
import { EditorModule } from 'primeng/editor';
import { MapTagByRangeDto } from './MapTagByRangeDto';
import { ProductDto } from '@shared/service-proxies/product-machine-price-service-proxies';
import { result } from 'lodash';

@Component({
  selector: 'mapTagByRange',
  templateUrl: './map-tag-by-ranage.component.html',
  styleUrls: ['./map-tag-by-ranage.component.css']
})
export class MapTagByRanageComponent extends AppComponentBase {
  @ViewChild('createOrEditModal') modal: ModalDirective;
  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
  baseRemoteUrl: string;

  model: MapTagByRangeDto;
  active = false;
  saving = false;
  products = new Array<ProductDto>();
  productId: string = "";
  constructor(
    injector: Injector,
    private _productsServiceProxy: ProductsServiceProxy,
    private _productTagsServiceProxy: ProductTagsServiceProxy,
    @Optional() @Inject(API_BASE_URL) baseRemoteUrl?: string,
  ) {
    super(injector);
    this.baseRemoteUrl = baseRemoteUrl ? baseRemoteUrl : "";
    this.model = new MapTagByRangeDto();
  }

  save(): void {
    var tenatnId = abp.multiTenancy.getTenantIdCookie();

    var insertTagsDto = new ListProductTagDto();
    insertTagsDto.tenantId = tenatnId;;
    let tags = new Array<ProductTagInputDto>();

    var prefix = this.model.prefix;
    var startTag = this.model.startTag;
    var endtag = this.model.endTag;
    var numberOfTag = this.model.numberOfTags;

    if (prefix != null && startTag != null && numberOfTag > 0) {
      var nStartTag = parseInt(startTag, 16);

      for (var i = 0; i < numberOfTag; i++) {
        let item = new ProductTagInputDto();
        item.productId = this.productId;

        var tagNumber = this.toHex(nStartTag + i);
        var tagId = this.model.prefix + this.padLeft(tagNumber, '0', 6);
        item.name = tagId;

        console.log(tagId);
        tags.push(item);
      }
      insertTagsDto.listTags = tags;
      var json  = JSON.stringify(insertTagsDto);
      console.log(json);
    }


    this._productTagsServiceProxy.insertTags(insertTagsDto).subscribe(result => {
      console.log(result);
      this.modal.hide();
      this.modalSave.emit(null);
    }, error => console.log(error));
  }


  close(): void {
    this.active = false;
    this.modal.hide();
  }

  show() {
    this.modal.show();
  }

  buildTag(): void {
    var prefix = this.model.prefix;
    var startTag = this.model.startTag;
    var endtag = this.model.endTag;
    var numberOfTag = this.model.numberOfTags;

    if (prefix != null && startTag != null && numberOfTag > 0) {
      var nStartTag = parseInt(startTag, 16);
      var hexStartTag = this.toHex(nStartTag);

      var nEndTag = nStartTag + numberOfTag;
      var hexEndTag = this.toHex(nEndTag - 1 );
      this.model.endTag = hexEndTag;


      hexEndTag = this.padLeft(hexEndTag, '0', 6);
      hexStartTag = this.padLeft(hexStartTag, '0', 6);

      var finalEndTag = prefix + hexEndTag;
      var finalStartTag = prefix + hexStartTag;

      console.log(finalEndTag);
      this.model.tagListStart = finalStartTag;
      this.model.tagListEnd = finalEndTag;
    }
  }

  toHex(d) {
    return ((Number(d).toString(16))).toUpperCase()
  }

  padLeft(text: string, padChar: string, size: number): string {
    return (String(padChar).repeat(size) + text).substr((size * -1), size);
  }

  getAllProducts() {
    this._productsServiceProxy.getAllProductsNoFilter().subscribe(data => {
      this.products = data.items;
    });
  }
  ngOnInit() {
    this.model.numberOfTags = 10;
    this.getAllProducts();
  }

}

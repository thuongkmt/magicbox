import { Component, OnInit, Injector, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { Pipe, PipeTransform } from '@angular/core';
import { MachineServiceProxy } from "shared/service-proxies/machine-service-proxies";
import { ProductsServiceProxy } from '@shared/service-proxies/product-service-proxies';
import { ProductMachinePriceServiceProxy, ProductDto, UpdateProductMachinePriceInput} from '@shared/service-proxies/product-machine-price-service-proxies';
import { FilterPipe } from '../../filter.pipe';
import {FilterMachineNamePipe} from '../../filter.pipe';
import { ProductCategoryDto, CategoryServiceProxy } from '@shared/service-proxies/category-service-proxies';

@Component({
  selector: 'app-products-machine',
  templateUrl: './products-machine.component.html',
  styleUrls: ['./products-machine.component.css'],
  animations: [appModuleAnimation()],
  providers: [FilterPipe,FilterMachineNamePipe]
})

export class ProductsMachineComponent extends AppComponentBase implements OnInit {
  @ViewChild('dataTable') dataTable: Table;
  @ViewChild('paginator') paginator: Paginator;

  listMachine = [];
  currentMachineId = '';
  currentMachineName = '';
  selected = '0';
  searchString = '';
  categoryId = '00000000-0000-0000-0000-000000000000';
  categories = new Array<ProductCategoryDto>();
  eventTable?: LazyLoadEvent;

  constructor(
    injector: Injector,
    private _machinesService: MachineServiceProxy,
    private _productsServiceProxy: ProductsServiceProxy,
    private _productsMachinePriceService: ProductMachinePriceServiceProxy,
    private _categoriesServiceProxy : CategoryServiceProxy) {
    super(injector);
  }

  ngOnInit() {
    // Get all machines.
    this.getMachines();
    this.getCategories();
  }

  //Get all categories
  getCategories(): void {
    let self = this;
    let categories = [];
    categories.push({id : '00000000-0000-0000-0000-000000000000',name : '-- All --'});
    this._categoriesServiceProxy.getAll("","","","",null,0,1000)
        .subscribe(
            (result) =>
                result.items.forEach(item => {
                    let category = new ProductCategoryDto();
                    category.id = item.id;
                    category.name = item.name;
                    categories.push(category);
        }));

        self.categories = categories;
}
  
  // Get all machines.
  getMachines(): void {
    this.primengTableHelper.showLoadingIndicator();
    this._productsMachinePriceService.getAllMachines(0, 99999, 'Name').subscribe((result) => {
      let machines = [];
      for (let entry of result.items) {
        let machine = {
          'machineId': entry.id,
          'machineName': entry.name
        };
        machines.push(machine);
      }
      this.listMachine = machines;
      this.primengTableHelper.hideLoadingIndicator();
    });
  }

  // Event when choose machine.
  chooseMachine(machineId: string, machineName: string, index: string) {
    this.categoryId = '00000000-0000-0000-0000-000000000000';
    this.primengTableHelper.showLoadingIndicator();
    // Set current machine.
    this.currentMachineId = machineId;
    this.currentMachineName = machineName;
    this.selected = index;

    // Get product by machine.
    this._productsMachinePriceService.getProductMachinePrices(
      machineId,
      this.categoryId,
      this.primengTableHelper.getSorting(this.dataTable),
      this.primengTableHelper.getSkipCount(this.paginator, this.eventTable),
      this.primengTableHelper.getMaxResultCount(this.paginator, this.eventTable)
      )
      .subscribe(result => {
        this.primengTableHelper.totalRecordsCount = result.totalCount;
        this.primengTableHelper.records = result.items;
        this.primengTableHelper.hideLoadingIndicator();
      });
  }

  loadProducts(event?: LazyLoadEvent) {
    // Get event table.
    this.eventTable = event;
    this.primengTableHelper.showLoadingIndicator();
    // Get product by machine.
    this._productsMachinePriceService.getProductMachinePrices(
      this.currentMachineId,
      this.categoryId,
      this.primengTableHelper.getSorting(this.dataTable),
      this.primengTableHelper.getSkipCount(this.paginator, event),
      this.primengTableHelper.getMaxResultCount(this.paginator, event)
      )
      .subscribe(result => {
        this.primengTableHelper.totalRecordsCount = result.totalCount;
        this.primengTableHelper.records = result.items;
        this.primengTableHelper.hideLoadingIndicator();
      });
  }

  // Update price with machine and product.
  updateProductMachinePrices(record: ProductDto) {
    if (record === null || record === undefined) {
      return;
    }
    if (record.price < 0) {
      this.notify.warn(this.l('PriceZero'));
      return;
    }
    let input = new UpdateProductMachinePriceInput();
    input.machineId = this.currentMachineId;
    input.productId = record.id;
    input.price = record.price;

    if (record && record.price === null) {
      this.notify.warn('Price is not empty.');
      return;
    }

    this._productsMachinePriceService.updateProductMachinePrices(input).subscribe(result => {
      if (result === true) {
        this.notify.success(this.l('SavedSuccessfully'));
      } else {
        this.notify.error(this.l('SaveFailed'));
      }
    });
  }
}


import { BlackListCardServiceProxiesService } from '@shared/service-proxies/black-list-card-service-proxies.service'
import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { Table } from 'primeng/table';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { CreateEditBlackListCardComponent } from '@app/main/transactions/create-edit-black-list-card/create-edit-black-list-card.component';

@Component({
  templateUrl: './black-list-cards.component.html',
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})

export class BlackListCardsComponent extends AppComponentBase {

  @ViewChild('dataTable') dataTable: Table;
  @ViewChild('paginator') paginator: Paginator;
  @ViewChild('createOrEditBlackListCardModal') createOrEditBlackListCardModal: CreateEditBlackListCardComponent;

  constructor(
              injector: Injector,
              private _blackListCardServiceProxy: BlackListCardServiceProxiesService
            ) 
  { 
    super(injector);
  }

  ngOnInit() {
  }

  getAll(event?: LazyLoadEvent) {
    if (this.primengTableHelper.shouldResetPaging(event)) {
        this.paginator.changePage(0);
        return;
    }

    this.primengTableHelper.showLoadingIndicator();

    this._blackListCardServiceProxy.getAll(
        this.primengTableHelper.getSkipCount(this.paginator, event),
        this.primengTableHelper.getMaxResultCount(this.paginator, event),
        this.primengTableHelper.getSorting(this.dataTable),
        
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    reloadPage(): void {
        this.paginator.changePage(this.paginator.getPage());
    }

    createNew(): void {
        this.createOrEditBlackListCardModal.show();
    }

    delete(id: string): void {
        this.message.confirm(
            '',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._blackListCardServiceProxy.delete(id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }
}

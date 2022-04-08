import { Component, ViewChild, Injector, Output, EventEmitter, OnInit } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { RestockSessionsServiceProxy, GetRestockSessionForViewDto, RestockSessionDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ActivatedRoute } from '@angular/router';
import { appModuleAnimation } from '@shared/animations/routerTransition';

@Component({
    templateUrl: './view-restockSession.component.html',
    animations: [appModuleAnimation()]
})
export class ViewRestockSessionComponent extends AppComponentBase implements OnInit {

    active = false;
    saving = false;

    item: GetRestockSessionForViewDto;


    constructor(
        injector: Injector,
        private _activatedRoute: ActivatedRoute,
         private _restockSessionsServiceProxy: RestockSessionsServiceProxy
    ) {
        super(injector);
        this.item = new GetRestockSessionForViewDto();
        this.item.restockSession = new RestockSessionDto();        
    }

    ngOnInit(): void {
        this.show(this._activatedRoute.snapshot.queryParams['id']);
    }

    show(restockSessionId: number): void {
      this._restockSessionsServiceProxy.getRestockSessionForView(restockSessionId).subscribe(result => {      
                 this.item = result;
                this.active = true;
            });       
    }
}

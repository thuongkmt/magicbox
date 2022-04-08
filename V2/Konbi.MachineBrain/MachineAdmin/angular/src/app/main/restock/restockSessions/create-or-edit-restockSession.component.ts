import { Component, ViewChild, Injector, Output, EventEmitter, OnInit} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { RestockSessionsServiceProxy, CreateOrEditRestockSessionDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import * as moment from 'moment';
import { ActivatedRoute, Router } from '@angular/router';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {Observable} from "@node_modules/rxjs";


@Component({
    templateUrl: './create-or-edit-restockSession.component.html',
    animations: [appModuleAnimation()]
})
export class CreateOrEditRestockSessionComponent extends AppComponentBase implements OnInit {
    active = false;
    saving = false;
    
    restockSession: CreateOrEditRestockSessionDto = new CreateOrEditRestockSessionDto();




    constructor(
        injector: Injector,
        private _activatedRoute: ActivatedRoute,        
        private _restockSessionsServiceProxy: RestockSessionsServiceProxy,
        private _router: Router
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.show(this._activatedRoute.snapshot.queryParams['id']);
    }

    show(restockSessionId?: number): void {

        if (!restockSessionId) {
            this.restockSession = new CreateOrEditRestockSessionDto();
            this.restockSession.id = restockSessionId;
            this.restockSession.startDate = moment().startOf('day');
            this.restockSession.endDate = moment().startOf('day');

            this.active = true;
        } else {
            this._restockSessionsServiceProxy.getRestockSessionForEdit(restockSessionId).subscribe(result => {
                this.restockSession = result.restockSession;


                this.active = true;
            });
        }
        
    }

    private saveInternal(): Observable<void> {
            this.saving = true;
            
        
        return this._restockSessionsServiceProxy.createOrEdit(this.restockSession)
         .pipe(finalize(() => { 
            this.saving = false;               
            this.notify.info(this.l('SavedSuccessfully'));
         }));
    }
    
    save(): void {
        this.saveInternal().subscribe(x => {
             this._router.navigate( ['/app/main/restock/restockSessions']);
        })
    }
    
    saveAndNew(): void {
        this.saveInternal().subscribe(x => {
            this.restockSession = new CreateOrEditRestockSessionDto();
        })
    }







}

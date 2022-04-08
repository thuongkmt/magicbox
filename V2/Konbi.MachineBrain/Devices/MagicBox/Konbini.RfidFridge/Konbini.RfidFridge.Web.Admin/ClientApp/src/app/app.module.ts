import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { NgbModule, ModalDismissReasons  } from '@ng-bootstrap/ng-bootstrap';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { DiagnosticComponent } from './diagnostic/diagnostic.component';
import { MessagesComponent } from './messages/messages.component';
import { TopupComponent } from './topup/topup.component';
import { ProductComponent } from './product/product.component';
import { SettingComponent } from './setting/setting.component';
import { TransactionComponent } from './transaction/transaction.component';
import { InventoryComponent } from './inventory/inventory.component';

// External modules
import { InjectableRxStompConfig, RxStompService, rxStompServiceFactory } from '@stomp/ng2-stompjs';
import { myRxStompConfig } from './my-rx-stomp.config';
import { AngularFontAwesomeModule } from 'angular-font-awesome';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    CounterComponent,
    FetchDataComponent,
    DiagnosticComponent,
    MessagesComponent,
    TopupComponent,
    ProductComponent,
    SettingComponent,
    TransactionComponent,
    InventoryComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    NgbModule,
    AngularFontAwesomeModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'counter', component: CounterComponent },
      { path: 'fetch-data1', component: FetchDataComponent },
      { path: 'diagnostic', component: DiagnosticComponent },
      { path: 'message', component: MessagesComponent },
      { path: 'topup', component: TopupComponent },
      { path: 'product', component: ProductComponent },
      { path: 'inventory', component: InventoryComponent },

    ])
  ],
  providers: [

    {
      provide: InjectableRxStompConfig,
      useValue: myRxStompConfig
    },
    {
      provide: RxStompService,
      useFactory: rxStompServiceFactory,
      deps: [InjectableRxStompConfig]
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

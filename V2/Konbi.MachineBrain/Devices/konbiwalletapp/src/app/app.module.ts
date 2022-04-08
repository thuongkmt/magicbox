import { BrowserModule } from '@angular/platform-browser';
import { NgModule, Injector, APP_INITIALIZER } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HttpClientModule } from  '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

/*...*/
import { MatToolbarModule } from  '@angular/material/toolbar';
import { MatCardModule } from  '@angular/material/card';
import { MatButtonModule } from  '@angular/material/button';
import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment';

import { ZXingScannerModule } from '@zxing/ngx-scanner';
import { NavComponent } from './nav/nav.component';
import { LayoutModule } from '@angular/cdk/layout';
import { MatTabsModule,MatSidenavModule, MatIconModule, MatListModule, MatDialogModule } from '@angular/material';
import { RoutingModule } from './routing/routing.module';
import { HeaderComponent } from './navigation/header/header.component';
import { HomeComponent } from './home/home.component';
import { LayoutComponent } from './layout/layout.component';

import { SidenavListComponent } from './navigation/sidenav-list/sidenav-list.component';
// import { MatTabsModule } from '@angular/material';
import { MatMenuModule} from '@angular/material/menu';
import { CreditComponent, SelectActionDialogComponent,TransactionSuccessDialogComponent,TransactionStartDialogComponent,TopupStartDialogComponent } from './credit/credit.component';

import { ScanqrComponent } from './scanqr/scanqr.component';
import { InjectableRxStompConfig, RxStompService, rxStompServiceFactory } from '@stomp/ng2-stompjs';
import { myRxStompConfig } from '../assets/my-rx-stomp.config';
import { MessagesComponent } from './messages/messages.component';
import { AppPreBootstrap } from './AppPreBootstrap';
import { PlatformLocation, HashLocationStrategy, LocationStrategy } from '@angular/common';
import { AppConsts } from './AppConsts';
import { CreditHistoryComponent } from './credit-history/credit-history.component';

//firebase.initializeApp(firebaseConfig);
 
export function getBaseHref(platformLocation: PlatformLocation): string {
  let baseUrl = platformLocation.getBaseHrefFromDOM();
  if (baseUrl) {
      return baseUrl;
  }

  return '/';
}


function getDocumentOrigin() {
  if (!document.location.origin) {
      return document.location.protocol + '//' + document.location.hostname + (document.location.port ? ':' + document.location.port : '');
  }

  return document.location.origin;
}

export function appInitializerFactory(
  injector: Injector,
  platformLocation: PlatformLocation) {
  return () => {
      return new Promise<boolean>((resolve, reject) => {
          AppConsts.appBaseHref = getBaseHref(platformLocation);
          let appBaseUrl = getDocumentOrigin() + AppConsts.appBaseHref;
          console.log('pre initialize app');
          AppPreBootstrap.run(appBaseUrl, ()=>{
            console.log('init finished');
            resolve(true);
          }, resolve, reject);
         
      });
  };
}




@NgModule({
  declarations: [
    AppComponent,
    NavComponent,
    HeaderComponent,
    LayoutComponent,
    HomeComponent,
    SidenavListComponent,
    CreditComponent,
    ScanqrComponent,
    SelectActionDialogComponent,
    TransactionSuccessDialogComponent,
    TransactionStartDialogComponent,
    TopupStartDialogComponent,
    MessagesComponent,
    CreditHistoryComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    BrowserAnimationsModule,
    MatToolbarModule,
    MatCardModule,
    MatButtonModule,
    ZXingScannerModule,
    ServiceWorkerModule.register('ngsw-worker.js', { enabled: environment.production }),
    LayoutModule,
    MatSidenavModule,
    MatIconModule,
    MatListModule,
    MatSidenavModule,
    MatTabsModule,
    MatMenuModule,
    RoutingModule,
    MatDialogModule
  ],
  entryComponents:[SelectActionDialogComponent,
    TransactionSuccessDialogComponent,
    TransactionStartDialogComponent,
    TopupStartDialogComponent],
  exports:[MatToolbarModule],
  providers: [
    {provide: LocationStrategy, useClass: HashLocationStrategy},
    {
      provide: InjectableRxStompConfig,
      useValue: myRxStompConfig
    },
    {
      provide: RxStompService,
      useFactory: rxStompServiceFactory,
      deps: [InjectableRxStompConfig]
    },
    {
        provide: APP_INITIALIZER,
        useFactory: appInitializerFactory,
        deps: [Injector, PlatformLocation],
        multi: true
    },
    // { provide: API_BASE_URL, useFactory: getRemoteServiceBaseUrl }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

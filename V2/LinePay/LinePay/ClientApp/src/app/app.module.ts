import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { WaitingComponent } from './waiting/waiting.component';
import { ErrorLinepayComponent } from './error-linepay/error-linepay.component';

import { NgxSpinnerModule } from "ngx-spinner";
import { FinishLinepayComponent } from './finish-linepay/finish-linepay.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    WaitingComponent,
    ErrorLinepayComponent,
    FinishLinepayComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'waiting', component: WaitingComponent },
      { path: 'finish-linepay', component: FinishLinepayComponent },
      { path: 'error-linepay', component: ErrorLinepayComponent },
    ]),
    NgxSpinnerModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }

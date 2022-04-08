import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from '../home/home.component';
import {CreditComponent} from '../credit/credit.component';
import { CreditHistoryComponent } from '../credit-history/credit-history.component';

const routes: Routes = [
  { path: 'home', component: CreditComponent},
  { path: 'credit', component: CreditComponent},
  { path: 'credit-history', component: CreditHistoryComponent},
  { path: '', redirectTo: '/credit', pathMatch: 'full' }
 
];


@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    RouterModule.forRoot(routes)
  ],
  exports: [
    RouterModule
  ],
  // declarations: []
})
export class RoutingModule { }

/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { FinishLinepayComponent } from './finish-linepay.component';

describe('FinishLinepayComponent', () => {
  let component: FinishLinepayComponent;
  let fixture: ComponentFixture<FinishLinepayComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FinishLinepayComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FinishLinepayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreditHistoryComponent } from './credit-history.component';

describe('CreditHistoryComponent', () => {
  let component: CreditHistoryComponent;
  let fixture: ComponentFixture<CreditHistoryComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreditHistoryComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreditHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MagicboxTransactionComponent } from './magicbox-transaction.component';

describe('MagicboxTransactionComponent', () => {
  let component: MagicboxTransactionComponent;
  let fixture: ComponentFixture<MagicboxTransactionComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MagicboxTransactionComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MagicboxTransactionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

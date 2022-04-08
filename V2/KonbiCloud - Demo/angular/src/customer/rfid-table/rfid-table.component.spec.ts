import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RfidTableComponent } from './rfid-table.component';

describe('RfidTableComponent', () => {
  let component: RfidTableComponent;
  let fixture: ComponentFixture<RfidTableComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RfidTableComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RfidTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

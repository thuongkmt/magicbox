import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RfidtableSettingComponent } from './rfidtable-setting.component';

describe('RfidtableSettingComponent', () => {
  let component: RfidtableSettingComponent;
  let fixture: ComponentFixture<RfidtableSettingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RfidtableSettingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RfidtableSettingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

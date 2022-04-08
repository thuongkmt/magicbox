import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AbpSettingComponent } from './abp-setting.component';

describe('AbpSettingComponent', () => {
  let component: AbpSettingComponent;
  let fixture: ComponentFixture<AbpSettingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AbpSettingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AbpSettingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

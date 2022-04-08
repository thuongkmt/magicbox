import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SyncSettingComponent } from './sync-setting.component';

describe('SyncSettingComponent', () => {
  let component: SyncSettingComponent;
  let fixture: ComponentFixture<SyncSettingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SyncSettingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SyncSettingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

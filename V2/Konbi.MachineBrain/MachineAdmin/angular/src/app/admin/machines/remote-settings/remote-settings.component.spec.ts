import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RemoteSettingsComponent } from './remote-settings.component';

describe('RemoteSettingsComponent', () => {
  let component: RemoteSettingsComponent;
  let fixture: ComponentFixture<RemoteSettingsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RemoteSettingsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RemoteSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

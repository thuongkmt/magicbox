import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShowDevicesComponent } from './show-devices.component';

describe('ShowDevicesComponent', () => {
  let component: ShowDevicesComponent;
  let fixture: ComponentFixture<ShowDevicesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ShowDevicesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShowDevicesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

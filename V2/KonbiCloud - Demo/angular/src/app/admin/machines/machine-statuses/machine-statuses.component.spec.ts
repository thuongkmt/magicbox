import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MachineStatusesComponent } from './machine-statuses.component';

describe('MachineStatusesComponent', () => {
  let component: MachineStatusesComponent;
  let fixture: ComponentFixture<MachineStatusesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MachineStatusesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MachineStatusesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

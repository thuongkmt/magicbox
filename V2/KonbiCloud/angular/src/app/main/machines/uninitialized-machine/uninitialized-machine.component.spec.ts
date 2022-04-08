import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UninitializedMachineComponent } from './uninitialized-machine.component';

describe('UninitializedMachineComponent', () => {
  let component: UninitializedMachineComponent;
  let fixture: ComponentFixture<UninitializedMachineComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UninitializedMachineComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UninitializedMachineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

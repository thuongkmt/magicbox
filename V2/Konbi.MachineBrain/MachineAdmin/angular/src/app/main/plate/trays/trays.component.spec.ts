import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TraysComponent } from './trays.component';

describe('TraysComponent', () => {
  let component: TraysComponent;
  let fixture: ComponentFixture<TraysComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TraysComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TraysComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

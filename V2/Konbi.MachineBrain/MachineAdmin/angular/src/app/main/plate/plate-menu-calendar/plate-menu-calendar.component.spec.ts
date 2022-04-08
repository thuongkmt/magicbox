import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PlateMenuCalendarComponent } from './plate-menu-calendar.component';

describe('PlateMenuCalendarComponent', () => {
  let component: PlateMenuCalendarComponent;
  let fixture: ComponentFixture<PlateMenuCalendarComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PlateMenuCalendarComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlateMenuCalendarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

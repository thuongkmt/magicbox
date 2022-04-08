import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PlateMenuDayComponent } from './plate-menu-day.component';

describe('PlateMenuDayComponent', () => {
  let component: PlateMenuDayComponent;
  let fixture: ComponentFixture<PlateMenuDayComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PlateMenuDayComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlateMenuDayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PlateMenusComponent } from './plate-menus.component';

describe('PlateMenusComponent', () => {
  let component: PlateMenusComponent;
  let fixture: ComponentFixture<PlateMenusComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PlateMenusComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlateMenusComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MapTagByRanageComponent } from './map-tag-by-ranage.component';

describe('MapTagByRanageComponent', () => {
  let component: MapTagByRanageComponent;
  let fixture: ComponentFixture<MapTagByRanageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MapTagByRanageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MapTagByRanageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

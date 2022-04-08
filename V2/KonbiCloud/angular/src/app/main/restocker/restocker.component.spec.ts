import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RestockerComponent } from './restocker.component';

describe('RestockerComponent', () => {
  let component: RestockerComponent;
  let fixture: ComponentFixture<RestockerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RestockerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RestockerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

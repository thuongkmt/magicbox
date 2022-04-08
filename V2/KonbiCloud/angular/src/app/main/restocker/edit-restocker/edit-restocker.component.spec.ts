import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EditRestockerComponent } from './edit-restocker.component';

describe('EditRestockerComponent', () => {
  let component: EditRestockerComponent;
  let fixture: ComponentFixture<EditRestockerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EditRestockerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EditRestockerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

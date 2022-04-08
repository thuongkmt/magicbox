import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MtDetailComponent } from './mt-detail.component';

describe('MtDetailComponent', () => {
  let component: MtDetailComponent;
  let fixture: ComponentFixture<MtDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MtDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MtDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

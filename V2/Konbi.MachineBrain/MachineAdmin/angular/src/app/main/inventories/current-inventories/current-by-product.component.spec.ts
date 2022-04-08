import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CurrentByProductComponent } from './current-by-product.component';

describe('CurrentByProductComponent', () => {
  let component: CurrentByProductComponent;
  let fixture: ComponentFixture<CurrentByProductComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CurrentByProductComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CurrentByProductComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

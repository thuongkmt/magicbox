import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportProductModelComponent } from './import-product-model.component';

describe('ImportProductModelComponent', () => {
  let component: ImportProductModelComponent;
  let fixture: ComponentFixture<ImportProductModelComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ImportProductModelComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ImportProductModelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

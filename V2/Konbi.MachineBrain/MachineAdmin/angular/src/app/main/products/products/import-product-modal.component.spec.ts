import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportProductModalComponent } from './import-product-modal.component';

describe('ImportProductModalComponent', () => {
  let component: ImportProductModalComponent;
  let fixture: ComponentFixture<ImportProductModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ImportProductModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ImportProductModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

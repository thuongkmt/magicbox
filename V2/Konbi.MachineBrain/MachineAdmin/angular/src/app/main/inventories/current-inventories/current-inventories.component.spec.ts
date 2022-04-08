import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CurrentInventoriesComponent } from './current-inventories.component';

describe('CurrentInventoriesComponent', () => {
  let component: CurrentInventoriesComponent;
  let fixture: ComponentFixture<CurrentInventoriesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CurrentInventoriesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CurrentInventoriesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

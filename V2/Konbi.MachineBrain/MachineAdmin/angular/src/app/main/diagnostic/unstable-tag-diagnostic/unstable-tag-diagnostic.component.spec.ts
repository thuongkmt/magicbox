import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UnstableTagDiagnosticComponent } from './unstable-tag-diagnostic.component';

describe('UnstableTagDiagnosticComponent', () => {
  let component: UnstableTagDiagnosticComponent;
  let fixture: ComponentFixture<UnstableTagDiagnosticComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UnstableTagDiagnosticComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UnstableTagDiagnosticComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

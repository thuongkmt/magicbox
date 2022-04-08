import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MdbcashlessDiagnosticComponent } from './mdbcashless-diagnostic.component';

describe('MdbcashlessDiagnosticComponent', () => {
  let component: MdbcashlessDiagnosticComponent;
  let fixture: ComponentFixture<MdbcashlessDiagnosticComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MdbcashlessDiagnosticComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MdbcashlessDiagnosticComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

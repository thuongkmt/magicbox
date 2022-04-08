import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RfidTableSettingComponent } from './rfid-table-setting.component';

describe('RfidTableSettingComponent', () => {
  let component: RfidTableSettingComponent;
  let fixture: ComponentFixture<RfidTableSettingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RfidTableSettingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RfidTableSettingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

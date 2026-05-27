import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsAlertComponent } from './ds-alert.component';

describe('DsAlertComponent', () => {
  let fixture: ComponentFixture<DsAlertComponent>;
  let component: DsAlertComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsAlertComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsAlertComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});

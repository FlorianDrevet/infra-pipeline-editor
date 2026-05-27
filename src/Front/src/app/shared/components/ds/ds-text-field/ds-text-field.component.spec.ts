import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsTextFieldComponent } from './ds-text-field.component';

describe('DsTextFieldComponent', () => {
  let fixture: ComponentFixture<DsTextFieldComponent>;
  let component: DsTextFieldComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsTextFieldComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsTextFieldComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});

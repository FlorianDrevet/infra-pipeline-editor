import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsCheckboxComponent } from './ds-checkbox.component';

describe('DsCheckboxComponent', () => {
  let fixture: ComponentFixture<DsCheckboxComponent>;
  let component: DsCheckboxComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsCheckboxComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsCheckboxComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});

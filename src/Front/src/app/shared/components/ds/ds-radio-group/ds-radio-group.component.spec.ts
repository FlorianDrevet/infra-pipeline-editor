import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsRadioGroupComponent } from './ds-radio-group.component';

describe('DsRadioGroupComponent', () => {
  let fixture: ComponentFixture<DsRadioGroupComponent>;
  let component: DsRadioGroupComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsRadioGroupComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsRadioGroupComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('options', [{ value: 'a', label: 'Option A' }]);
    fixture.componentRef.setInput('name', 'test-radio');
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});

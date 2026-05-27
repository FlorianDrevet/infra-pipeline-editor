import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsSegmentedControlComponent } from './ds-segmented-control.component';

describe('DsSegmentedControlComponent', () => {
  let fixture: ComponentFixture<DsSegmentedControlComponent>;
  let component: DsSegmentedControlComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsSegmentedControlComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsSegmentedControlComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('options', [{ value: 'tab1', label: 'Tab 1' }]);
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});

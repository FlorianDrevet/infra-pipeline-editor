import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsTooltipComponent } from './ds-tooltip.component';

describe('DsTooltipComponent', () => {
  let fixture: ComponentFixture<DsTooltipComponent>;
  let component: DsTooltipComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsTooltipComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsTooltipComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('text', 'Tooltip text');
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});

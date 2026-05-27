import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsChipComponent } from './ds-chip.component';

describe('DsChipComponent', () => {
  let fixture: ComponentFixture<DsChipComponent>;
  let component: DsChipComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsChipComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsChipComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});

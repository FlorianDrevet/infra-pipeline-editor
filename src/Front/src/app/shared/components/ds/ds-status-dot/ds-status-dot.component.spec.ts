import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsStatusDotComponent } from './ds-status-dot.component';

describe('DsStatusDotComponent', () => {
  let fixture: ComponentFixture<DsStatusDotComponent>;
  let component: DsStatusDotComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsStatusDotComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsStatusDotComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsEmptyStateComponent } from './ds-empty-state.component';

describe('DsEmptyStateComponent', () => {
  let fixture: ComponentFixture<DsEmptyStateComponent>;
  let component: DsEmptyStateComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsEmptyStateComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsEmptyStateComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});

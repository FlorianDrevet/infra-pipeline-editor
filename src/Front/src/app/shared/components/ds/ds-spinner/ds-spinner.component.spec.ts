import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { DsSpinnerComponent } from './ds-spinner.component';

describe('DsSpinnerComponent', () => {
  let fixture: ComponentFixture<DsSpinnerComponent>;
  let component: DsSpinnerComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsSpinnerComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(DsSpinnerComponent);
    component = fixture.componentInstance;
  });

  it('should create with default md size', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    const root = fixture.nativeElement.querySelector('.ds-spinner') as HTMLElement;
    expect(root.classList).toContain('ds-spinner--md');
  });

  it('should apply requested size class', () => {
    fixture.componentRef.setInput('size', 'xl');
    fixture.detectChanges();
    const root = fixture.nativeElement.querySelector('.ds-spinner') as HTMLElement;
    expect(root.classList).toContain('ds-spinner--xl');
  });

  it('should expose role="status" for screen readers', () => {
    fixture.detectChanges();
    const root = fixture.nativeElement.querySelector('.ds-spinner') as HTMLElement;
    expect(root.getAttribute('role')).toBe('status');
    expect(root.getAttribute('aria-label')).toBeTruthy();
  });

  it('should use custom label when provided', () => {
    fixture.componentRef.setInput('label', 'Saving project');
    fixture.detectChanges();
    const root = fixture.nativeElement.querySelector('.ds-spinner') as HTMLElement;
    expect(root.getAttribute('aria-label')).toBe('Saving project');
  });
});

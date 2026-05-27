import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DsProgressBarComponent } from './ds-progress-bar.component';

describe('DsProgressBarComponent', () => {
  let fixture: ComponentFixture<DsProgressBarComponent>;
  let component: DsProgressBarComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsProgressBarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsProgressBarComponent);
    component = fixture.componentInstance;
  });

  it('should create with default determinate mode', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    const track = fixture.nativeElement.querySelector('.ds-progress__track') as HTMLElement;
    expect(track.getAttribute('role')).toBe('progressbar');
    expect(track.getAttribute('aria-valuenow')).toBe('0');
  });

  it('should clamp value to [0, 100]', () => {
    fixture.componentRef.setInput('value', 150);
    fixture.detectChanges();
    const track = fixture.nativeElement.querySelector('.ds-progress__track') as HTMLElement;
    expect(track.getAttribute('aria-valuenow')).toBe('100');
  });

  it('should not expose aria-valuenow in indeterminate mode', () => {
    fixture.componentRef.setInput('mode', 'indeterminate');
    fixture.detectChanges();
    const track = fixture.nativeElement.querySelector('.ds-progress__track') as HTMLElement;
    expect(track.getAttribute('aria-valuenow')).toBeNull();
    const bar = fixture.nativeElement.querySelector('.ds-progress__bar--indeterminate');
    expect(bar).toBeTruthy();
  });

  it('should apply tone modifier class', () => {
    fixture.componentRef.setInput('tone', 'success');
    fixture.detectChanges();
    const track = fixture.nativeElement.querySelector('.ds-progress__track') as HTMLElement;
    expect(track.classList).toContain('ds-progress__track--success');
  });
});

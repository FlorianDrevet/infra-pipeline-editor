import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { DsAccordionComponent } from './ds-accordion.component';

@Component({
  standalone: true,
  imports: [DsAccordionComponent],
  template: `
    <app-ds-accordion
      [title]="title()"
      [icon]="icon()"
      [tone]="tone()"
      [disabled]="disabled()"
      [(expanded)]="expanded">
      <p class="test-body">Body content</p>
    </app-ds-accordion>
  `,
})
class TestHostComponent {
  readonly title = signal('Test Title');
  readonly icon = signal<string | undefined>('school');
  readonly tone = signal<'neutral' | 'brand' | 'info'>('neutral');
  readonly disabled = signal(false);
  expanded = signal(false);
}

describe('DsAccordionComponent', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should render title and icon', () => {
    const title = fixture.debugElement.query(By.css('.ds-accordion__title'));
    expect(title.nativeElement.textContent.trim()).toBe('Test Title');

    const icon = fixture.debugElement.query(By.css('.ds-accordion__icon'));
    expect(icon.nativeElement.textContent.trim()).toBe('school');
  });

  it('should toggle expanded state on header click', () => {
    const header = fixture.debugElement.query(By.css('.ds-accordion__header'));

    expect(host.expanded()).toBe(false);
    expect(fixture.debugElement.query(By.css('.ds-accordion__body'))).toBeNull();

    header.nativeElement.click();
    fixture.detectChanges();

    expect(host.expanded()).toBe(true);
    expect(fixture.debugElement.query(By.css('.ds-accordion__body'))).not.toBeNull();
  });

  it('should show body content when expanded', () => {
    host.expanded.set(true);
    fixture.detectChanges();

    const body = fixture.debugElement.query(By.css('.test-body'));
    expect(body.nativeElement.textContent.trim()).toBe('Body content');
  });

  it('should not toggle when disabled', () => {
    host.disabled.set(true);
    fixture.detectChanges();

    const header = fixture.debugElement.query(By.css('.ds-accordion__header'));
    header.nativeElement.click();
    fixture.detectChanges();

    expect(host.expanded()).toBe(false);
    expect(fixture.debugElement.query(By.css('.ds-accordion__body'))).toBeNull();
  });

  it('should set aria-expanded attribute correctly', () => {
    const header = fixture.debugElement.query(By.css('.ds-accordion__header'));
    expect(header.nativeElement.getAttribute('aria-expanded')).toBe('false');

    header.nativeElement.click();
    fixture.detectChanges();

    expect(header.nativeElement.getAttribute('aria-expanded')).toBe('true');
  });

  it('should apply disabled class on root element', () => {
    host.disabled.set(true);
    fixture.detectChanges();

    const root = fixture.debugElement.query(By.css('.ds-accordion'));
    expect(root.nativeElement.classList.contains('ds-accordion--disabled')).toBe(true);
  });

  it('should rotate indicator when expanded', () => {
    const indicator = fixture.debugElement.query(By.css('.ds-accordion__indicator'));
    expect(indicator.nativeElement.classList.contains('ds-accordion__indicator--rotated')).toBe(false);

    host.expanded.set(true);
    fixture.detectChanges();

    expect(indicator.nativeElement.classList.contains('ds-accordion__indicator--rotated')).toBe(true);
  });

  it('should not render icon when undefined', () => {
    host.icon.set(undefined);
    fixture.detectChanges();

    const icon = fixture.debugElement.query(By.css('.ds-accordion__icon'));
    expect(icon).toBeNull();
  });
});

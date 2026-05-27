import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DsCardMatComponent } from './ds-card-mat.component';

@Component({
  standalone: true,
  imports: [DsCardMatComponent],
  template: `
    <app-ds-card-mat [title]="title" [subtitle]="subtitle" [tone]="tone">
      <span ds-card-header class="header-mark">HEADER</span>
      <div ds-card-content class="content-mark">CONTENT</div>
      <button ds-card-actions class="actions-mark">ACTION</button>
    </app-ds-card-mat>
  `,
})
class HostComponent {
  title?: string;
  subtitle?: string;
  tone: 'neutral' | 'brand' | 'success' | 'warning' | 'danger' = 'neutral';
}

describe('DsCardMatComponent', () => {
  let fixture: ComponentFixture<HostComponent>;
  let host: HostComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HostComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(HostComponent);
    host = fixture.componentInstance;
  });

  it('projects header, content and actions into the matching slots', () => {
    fixture.detectChanges();

    const root = getCardElement();
    expect(root.querySelector('.ds-card-mat__header .header-mark')?.textContent).toBe('HEADER');
    expect(root.querySelector('.ds-card-mat__content .content-mark')?.textContent).toBe('CONTENT');
    expect(root.querySelector('.ds-card-mat__actions .actions-mark')?.textContent).toBe('ACTION');
  });

  it('renders the title block only when title or subtitle is provided', () => {
    fixture.detectChanges();
    expect(getCardElement().querySelector('.ds-card-mat__title-block')).toBeNull();

    host.title = 'My title';
    host.subtitle = 'My subtitle';
    fixture.detectChanges();

    const titleBlock = getCardElement().querySelector('.ds-card-mat__title-block');
    expect(titleBlock).not.toBeNull();
    expect(titleBlock!.querySelector('.ds-card-mat__title')?.textContent).toBe('My title');
    expect(titleBlock!.querySelector('.ds-card-mat__subtitle')?.textContent).toBe('My subtitle');
  });

  it('applies the tone modifier class on the root element', () => {
    fixture.detectChanges();
    expect(getCardElement().classList).toContain('ds-card-mat--tone-neutral');

    host.tone = 'danger';
    fixture.detectChanges();
    expect(getCardElement().classList).toContain('ds-card-mat--tone-danger');
    expect(getCardElement().classList).not.toContain('ds-card-mat--tone-neutral');
  });

  function getCardElement(): HTMLElement {
    return fixture.nativeElement.querySelector('.ds-card-mat') as HTMLElement;
  }
});

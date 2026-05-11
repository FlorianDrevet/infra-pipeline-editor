import { OverlayContainer } from '@angular/cdk/overlay';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DsSelectComponent, DsSelectOption } from './ds-select.component';

@Component({
  standalone: true,
  imports: [DsSelectComponent],
  template: `
    <div class="host-shell">
      <app-ds-select [options]="options" placeholder="Choose one"></app-ds-select>
    </div>
  `,
  styles: [
    `
      .host-shell {
        width: 248px;
      }
    `,
  ],
})
class DsSelectHostComponent {
  public readonly options: DsSelectOption[] = [
    { value: 'name', label: 'Sort by name' },
    { value: 'members', label: 'Sort by members' },
    { value: 'favorites', label: 'Favorites first' },
  ];
}

describe('DsSelectComponent', () => {
  let fixture: ComponentFixture<DsSelectHostComponent>;
  let overlayContainer: OverlayContainer;
  let overlayElement: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsSelectHostComponent],
    }).compileComponents();

    overlayContainer = TestBed.inject(OverlayContainer);
    overlayElement = overlayContainer.getContainerElement();

    fixture = TestBed.createComponent(DsSelectHostComponent);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  afterEach(() => {
    overlayContainer.ngOnDestroy();
  });

  it('renders the dropdown panel with the same width as the trigger', async () => {
    const trigger = fixture.nativeElement.querySelector('.ds-select__trigger') as HTMLButtonElement | null;

    expect(trigger).withContext('select trigger should exist').not.toBeNull();

    trigger?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const panel = overlayElement.querySelector('.ds-select__panel') as HTMLElement | null;

    expect(panel).withContext('overlay panel should open').not.toBeNull();
    expect(Math.round(panel?.getBoundingClientRect().width ?? 0)).toBe(
      Math.round(trigger?.getBoundingClientRect().width ?? 0)
    );
  });
});
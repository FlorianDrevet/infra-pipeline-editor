import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  effect,
  input,
  model,
  output,
  signal,
  viewChildren,
} from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

import { DsChipComponent } from '../ds-chip/ds-chip.component';
import { DsTabDefinition } from './ds-tabs.types';

/**
 * Design system tabs (V3). Renders a horizontal `role="tablist"` driven by a
 * declarative `tabs` input and a two-way bound `activeTabId` signal.
 *
 * Keyboard navigation: ArrowLeft / ArrowRight cycle through enabled tabs,
 * Home / End jump to the first / last enabled tab. Only the active tab keeps
 * `tabindex="0"`; the others are `-1` for a single tab-stop on the tablist.
 */
@Component({
  selector: 'app-ds-tabs',
  standalone: true,
  imports: [MatIconModule, DsChipComponent],
  templateUrl: './ds-tabs.component.html',
  styleUrl: './ds-tabs.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsTabsComponent {
  public readonly tabs = input.required<readonly DsTabDefinition[]>();
  public readonly activeTabId = model<string | null>(null);
  public readonly ariaLabel = input<string | undefined>(undefined);
  public readonly stretch = input(false);

  public readonly tabChange = output<string>();

  protected readonly tabButtons = viewChildren<ElementRef<HTMLButtonElement>>('tabButton');

  private readonly focusIndex = signal<number | null>(null);

  protected readonly resolvedActiveId = computed<string | null>(() => {
    const explicit = this.activeTabId();
    if (explicit !== null && this.tabs().some((tab) => tab.id === explicit && !tab.disabled)) {
      return explicit;
    }
    const firstEnabled = this.tabs().find((tab) => !tab.disabled);
    return firstEnabled ? firstEnabled.id : null;
  });

  public constructor() {
    effect(() => {
      const focusAt = this.focusIndex();
      if (focusAt === null) {
        return;
      }
      const buttons = this.tabButtons();
      buttons[focusAt]?.nativeElement.focus();
      this.focusIndex.set(null);
    });
  }

  protected isActive(tab: DsTabDefinition): boolean {
    return tab.id === this.resolvedActiveId();
  }

  protected onSelect(tab: DsTabDefinition): void {
    if (tab.disabled || tab.id === this.activeTabId()) {
      return;
    }
    this.activeTabId.set(tab.id);
    this.tabChange.emit(tab.id);
  }

  protected onKeyDown(event: KeyboardEvent, currentIndex: number): void {
    const tabs = this.tabs();
    if (tabs.length === 0) {
      return;
    }

    let nextIndex: number | null = null;
    switch (event.key) {
      case 'ArrowRight':
        nextIndex = this.findNextEnabled(currentIndex, 1);
        break;
      case 'ArrowLeft':
        nextIndex = this.findNextEnabled(currentIndex, -1);
        break;
      case 'Home':
        nextIndex = this.findNextEnabled(-1, 1);
        break;
      case 'End':
        nextIndex = this.findNextEnabled(tabs.length, -1);
        break;
      default:
        return;
    }

    if (nextIndex === null || nextIndex === currentIndex) {
      return;
    }

    event.preventDefault();
    const target = tabs[nextIndex];
    this.activeTabId.set(target.id);
    this.tabChange.emit(target.id);
    this.focusIndex.set(nextIndex);
  }

  private findNextEnabled(fromIndex: number, direction: 1 | -1): number | null {
    const tabs = this.tabs();
    const length = tabs.length;
    for (let step = 1; step <= length; step++) {
      const candidate = fromIndex + direction * step;
      if (candidate < 0 || candidate >= length) {
        continue;
      }
      if (!tabs[candidate].disabled) {
        return candidate;
      }
    }
    return null;
  }
}

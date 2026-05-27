import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  ViewChildren,
  QueryList,
  inject,
  input,
  output,
} from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

import { DsMenuItem } from './ds-menu.types';

/**
 * Internal popover rendered by {@link DsMenuDirective}. Not intended for direct
 * use — consumers should attach `[appDsMenu]` to a trigger element.
 *
 * Provides full keyboard support (Arrow Up/Down, Home/End, Enter/Space, Escape)
 * and ARIA `role="menu"`/`menuitem` semantics.
 */
@Component({
  selector: 'app-ds-menu',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-menu.component.html',
  styleUrl: './ds-menu.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsMenuComponent implements AfterViewInit {
  public readonly items = input.required<readonly DsMenuItem[]>();
  public readonly ariaLabel = input<string | undefined>(undefined);

  public readonly itemSelected = output<DsMenuItem>();
  public readonly closeRequested = output<void>();

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  @ViewChildren('menuItem')
  private menuItemElements?: QueryList<ElementRef<HTMLButtonElement>>;

  public ngAfterViewInit(): void {
    this.focusFirstEnabled();
  }

  /** Programmatically focus the first non-disabled, non-divider item. */
  public focusFirstEnabled(): void {
    const elements = this.getInteractiveElements();
    if (elements.length > 0) {
      elements[0].focus();
    }
  }

  @HostListener('keydown', ['$event'])
  protected onKeyDown(event: KeyboardEvent): void {
    const elements = this.getInteractiveElements();
    if (elements.length === 0) {
      if (event.key === 'Escape') {
        this.closeRequested.emit();
      }
      return;
    }
    const activeIndex = elements.findIndex((el) => el === document.activeElement);

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.focusAt(elements, activeIndex + 1);
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.focusAt(elements, activeIndex - 1);
        break;
      case 'Home':
        event.preventDefault();
        elements[0].focus();
        break;
      case 'End':
        event.preventDefault();
        elements[elements.length - 1].focus();
        break;
      case 'Escape':
        event.preventDefault();
        this.closeRequested.emit();
        break;
      default:
        break;
    }
  }

  protected onItemClick(item: DsMenuItem): void {
    if (item.disabled || item.divider) {
      return;
    }
    this.itemSelected.emit(item);
  }

  private getInteractiveElements(): HTMLButtonElement[] {
    if (!this.menuItemElements) {
      return [];
    }
    return this.menuItemElements
      .toArray()
      .map((ref) => ref.nativeElement)
      .filter((el) => !el.disabled);
  }

  private focusAt(elements: HTMLButtonElement[], index: number): void {
    const length = elements.length;
    const normalized = ((index % length) + length) % length;
    elements[normalized].focus();
  }
}

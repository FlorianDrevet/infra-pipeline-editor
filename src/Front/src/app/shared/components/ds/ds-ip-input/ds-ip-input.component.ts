import {
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  forwardRef,
  input,
  signal,
  viewChildren,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import { DsIpInputMode } from './ds-ip-input.types';
import {
  getIpSegmentDefs,
  joinSegments,
  sanitizeSegment,
  shouldAdvanceSegment,
  splitToSegments,
} from './ds-ip-input.util';

type SegmentCaret = 'start' | 'end' | 'all';

/**
 * Design system segmented IPv4 / CIDR input. Renders one field per octet (plus a
 * prefix field in `cidr` mode) with the `.` and `/` separators shown as a
 * persistent mask. The user types digits only: focus auto-advances to the next
 * segment once the current one is full, and `Backspace` / arrows step back.
 *
 * Exposes the canonical `a.b.c.d` (or `a.b.c.d/p`) string through
 * {@link ControlValueAccessor} so it drops into Reactive Forms unchanged.
 */
@Component({
  selector: 'app-ds-ip-input',
  standalone: true,
  imports: [MatIconModule, TranslateModule],
  templateUrl: './ds-ip-input.component.html',
  styleUrl: './ds-ip-input.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsIpInputComponent),
      multi: true,
    },
  ],
})
export class DsIpInputComponent implements ControlValueAccessor {
  public readonly mode = input<DsIpInputMode>('ipv4');
  public readonly label = input<string | undefined>(undefined);
  public readonly hint = input<string | undefined>(undefined);
  public readonly error = input<string | undefined>(undefined);
  public readonly disabled = input<boolean>(false);
  public readonly required = input<boolean>(false);

  protected readonly defs = computed(() => getIpSegmentDefs(this.mode()));
  protected readonly prefixIcon = computed(() => (this.mode() === 'cidr' ? 'lan' : 'dns'));

  private readonly rawSegments = signal<string[]>([]);
  private readonly internalDisabled = signal(false);

  protected readonly disabledState = computed(() => this.disabled() || this.internalDisabled());

  /** Segment values padded / truncated to the current mode's segment count. */
  protected readonly segments = computed(() => {
    const current = this.rawSegments();
    return this.defs().map((_, index) => current[index] ?? '');
  });

  protected readonly segmentInputs = viewChildren<ElementRef<HTMLInputElement>>('segInput');

  private onChangeFn: (value: string) => void = () => {};
  private onTouchedFn: () => void = () => {};

  public writeValue(value: string | null): void {
    this.rawSegments.set(splitToSegments(value, this.mode()));
  }

  public registerOnChange(fn: (value: string) => void): void {
    this.onChangeFn = fn;
  }

  public registerOnTouched(fn: () => void): void {
    this.onTouchedFn = fn;
  }

  public setDisabledState(isDisabled: boolean): void {
    this.internalDisabled.set(isDisabled);
  }

  protected onSegmentInput(index: number, event: Event): void {
    const inputEl = event.target as HTMLInputElement;
    const def = this.defs()[index];
    const sanitized = sanitizeSegment(inputEl.value, def);

    const next = [...this.segments()];
    next[index] = sanitized;
    this.commit(next);

    if (inputEl.value !== sanitized) {
      inputEl.value = sanitized;
    }

    if (shouldAdvanceSegment(sanitized, def) && index < this.defs().length - 1) {
      this.focusSegment(index + 1, 'all');
    }
  }

  protected onSegmentKeydown(index: number, event: KeyboardEvent): void {
    const { key, ctrlKey, metaKey } = event;
    if (ctrlKey || metaKey) {
      return;
    }

    const inputEl = event.target as HTMLInputElement;
    const atStart = inputEl.selectionStart === 0 && inputEl.selectionEnd === 0;
    const atEnd = inputEl.selectionStart === inputEl.value.length;
    const isLast = index === this.defs().length - 1;

    if (key === '.' || key === '/') {
      event.preventDefault();
      if (!isLast) {
        this.focusSegment(index + 1, 'all');
      }
      return;
    }

    if (index > 0 && ((key === 'Backspace' && atStart) || (key === 'ArrowLeft' && atStart))) {
      event.preventDefault();
      this.focusSegment(index - 1, 'end');
      return;
    }

    if (!isLast && key === 'ArrowRight' && atEnd) {
      event.preventDefault();
      this.focusSegment(index + 1, 'start');
      return;
    }

    if (['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Enter'].includes(key)) {
      return;
    }

    if (!/^\d$/.test(key)) {
      event.preventDefault();
    }
  }

  protected onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const pasted = event.clipboardData?.getData('text') ?? '';
    const next = splitToSegments(pasted, this.mode());
    this.commit(next);

    const lastFilled = next.reduce((acc, value, index) => (value !== '' ? index : acc), 0);
    this.focusSegment(lastFilled, 'end');
  }

  protected onFocusOut(event: FocusEvent): void {
    const host = event.currentTarget as HTMLElement;
    const nextTarget = event.relatedTarget as Node | null;
    if (!nextTarget || !host.contains(nextTarget)) {
      this.onTouchedFn();
    }
  }

  private commit(next: string[]): void {
    this.rawSegments.set(next);
    this.onChangeFn(joinSegments(next, this.mode()));
  }

  private focusSegment(index: number, caret: SegmentCaret): void {
    const target = this.segmentInputs()[index]?.nativeElement;
    if (!target) {
      return;
    }

    target.focus();
    const length = target.value.length;
    if (caret === 'all') {
      target.select();
    } else if (caret === 'end') {
      target.setSelectionRange(length, length);
    } else {
      target.setSelectionRange(0, 0);
    }
  }
}

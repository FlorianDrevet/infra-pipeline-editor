import {
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  forwardRef,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

const MAX_OCTETS = 4;
const MAX_OCTET_VALUE = 255;
const MAX_PREFIX_VALUE = 32;

/**
 * Design system formatted CIDR block input (e.g. `10.0.0.0/16`).
 * Auto-inserts dots and slash as the user types. Validates octet 0-255
 * and prefix 0-32. Implements {@link ControlValueAccessor} for Reactive Forms.
 */
@Component({
  selector: 'app-ds-cidr-input',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-cidr-input.component.html',
  styleUrl: './ds-cidr-input.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsCidrInputComponent),
      multi: true,
    },
  ],
})
export class DsCidrInputComponent implements ControlValueAccessor {
  public readonly label = input<string | undefined>(undefined);
  public readonly placeholder = input<string>('0.0.0.0/0');
  public readonly hint = input<string | undefined>(undefined);
  public readonly error = input<string | undefined>(undefined);
  public readonly disabled = input<boolean>(false);
  public readonly required = input<boolean>(false);

  protected readonly value = signal<string>('');
  protected readonly focused = signal(false);
  private readonly internalDisabled = signal(false);

  protected readonly disabledState = computed(() => this.disabled() || this.internalDisabled());

  protected readonly inputRef = viewChild<ElementRef<HTMLInputElement>>('inputEl');

  private onChangeFn: (v: string) => void = () => {};
  private onTouchedFn: () => void = () => {};

  public writeValue(v: string | null): void {
    this.value.set(v ?? '');
  }

  public registerOnChange(fn: (v: string) => void): void {
    this.onChangeFn = fn;
  }

  public registerOnTouched(fn: () => void): void {
    this.onTouchedFn = fn;
  }

  public setDisabledState(isDisabled: boolean): void {
    this.internalDisabled.set(isDisabled);
  }

  protected onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const raw = input.value;
    const formatted = this.formatCidr(raw);

    this.value.set(formatted);
    this.onChangeFn(formatted);

    if (formatted !== raw) {
      input.value = formatted;
      input.setSelectionRange(formatted.length, formatted.length);
    }
  }

  protected onKeyDown(event: KeyboardEvent): void {
    const input = event.target as HTMLInputElement;
    const { key, ctrlKey, metaKey } = event;

    if (ctrlKey || metaKey) return;
    if (['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(key)) return;

    if (key === '.' || key === '/') {
      event.preventDefault();
      const cursor = input.selectionStart ?? 0;
      const current = input.value;
      const separator = this.getExpectedSeparator(current, cursor);
      if (separator) {
        const newVal = current.slice(0, cursor) + separator + current.slice(cursor);
        const formatted = this.formatCidr(newVal);
        this.value.set(formatted);
        this.onChangeFn(formatted);
        input.value = formatted;
        input.setSelectionRange(cursor + 1, cursor + 1);
      }
      return;
    }

    if (!/^\d$/.test(key)) {
      event.preventDefault();
    }
  }

  protected onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const pasted = event.clipboardData?.getData('text') ?? '';
    const cleaned = pasted.replace(/[^\d./]/g, '');
    const formatted = this.formatCidr(cleaned);
    const input = event.target as HTMLInputElement;

    this.value.set(formatted);
    this.onChangeFn(formatted);
    input.value = formatted;
    input.setSelectionRange(formatted.length, formatted.length);
  }

  protected onFocus(): void {
    this.focused.set(true);
  }

  protected onBlur(): void {
    this.focused.set(false);
    this.onTouchedFn();
  }

  private formatCidr(raw: string): string {
    const cleaned = raw.replace(/[^\d./]/g, '');
    const parts = cleaned.split(/[./]/);
    const result: string[] = [];
    let separators: string[] = [];

    for (let i = 0; i < parts.length && i <= MAX_OCTETS; i++) {
      let part = parts[i];
      if (i < MAX_OCTETS) {
        if (part.length > 3) part = part.slice(0, 3);
        const num = Number(part);
        if (part.length > 0 && num > MAX_OCTET_VALUE) part = String(MAX_OCTET_VALUE);
        result.push(part);
        if (i < MAX_OCTETS - 1) separators.push('.');
        else separators.push('/');
      } else {
        if (part.length > 2) part = part.slice(0, 2);
        const num = Number(part);
        if (part.length > 0 && num > MAX_PREFIX_VALUE) part = String(MAX_PREFIX_VALUE);
        result.push(part);
      }
    }

    let output = '';
    for (let i = 0; i < result.length; i++) {
      output += result[i];
      if (i < result.length - 1 && i < separators.length) {
        output += separators[i];
      }
    }

    return output;
  }

  private getExpectedSeparator(value: string, cursor: number): string | null {
    const beforeCursor = value.slice(0, cursor);
    const dotCount = (beforeCursor.match(/\./g) || []).length;
    const hasSlash = beforeCursor.includes('/');

    if (dotCount < 3 && !hasSlash) return '.';
    if (dotCount === 3 && !hasSlash) return '/';
    return null;
  }
}

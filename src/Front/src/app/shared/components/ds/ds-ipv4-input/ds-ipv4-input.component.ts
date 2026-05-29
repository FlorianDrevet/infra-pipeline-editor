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

/**
 * Design system formatted IPv4 address input (e.g. `10.0.0.4`).
 * Auto-inserts dots as the user types. Validates each octet 0-255.
 * Implements {@link ControlValueAccessor} for Reactive Forms.
 */
@Component({
  selector: 'app-ds-ipv4-input',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-ipv4-input.component.html',
  styleUrl: './ds-ipv4-input.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsIpv4InputComponent),
      multi: true,
    },
  ],
})
export class DsIpv4InputComponent implements ControlValueAccessor {
  public readonly label = input<string | undefined>(undefined);
  public readonly placeholder = input<string>('0.0.0.0');
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
    const formatted = this.formatIpv4(raw);

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

    if (key === '.') {
      event.preventDefault();
      const cursor = input.selectionStart ?? 0;
      const current = input.value;
      const dotCount = (current.slice(0, cursor).match(/\./g) || []).length;

      if (dotCount < 3) {
        const newVal = current.slice(0, cursor) + '.' + current.slice(cursor);
        const formatted = this.formatIpv4(newVal);
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
    const cleaned = pasted.replace(/[^\d.]/g, '');
    const formatted = this.formatIpv4(cleaned);
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

  private formatIpv4(raw: string): string {
    const cleaned = raw.replace(/[^\d.]/g, '');
    const parts = cleaned.split('.');
    const result: string[] = [];

    for (let i = 0; i < parts.length && i < MAX_OCTETS; i++) {
      let part = parts[i];
      if (part.length > 3) part = part.slice(0, 3);
      const num = Number(part);
      if (part.length > 0 && num > MAX_OCTET_VALUE) part = String(MAX_OCTET_VALUE);
      result.push(part);
    }

    return result.join('.');
  }
}

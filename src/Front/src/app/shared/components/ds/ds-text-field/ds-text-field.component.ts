import {
  ChangeDetectionStrategy,
  Component,
  computed,
  forwardRef,
  input,
  signal,
} from '@angular/core';

import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

let dsTextFieldUid = 0;

/**
 * Design system text input. Supports Reactive Forms (formControl), template-driven (ngModel)
 * and two-way value binding. Native input with full visual control (no mat-form-field).
 */
@Component({
  selector: 'app-ds-text-field',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-text-field.component.html',
  styleUrl: './ds-text-field.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsTextFieldComponent),
      multi: true,
    },
  ],
})
export class DsTextFieldComponent implements ControlValueAccessor {
  public readonly label = input<string | undefined>(undefined);
  public readonly placeholder = input<string>('');
  public readonly type = input<'text' | 'email' | 'password' | 'number' | 'tel' | 'url' | 'date'>('text');
  public readonly hint = input<string | undefined>(undefined);
  public readonly error = input<string | undefined>(undefined);
  public readonly disabled = input<boolean>(false);
  public readonly required = input<boolean>(false);
  public readonly min = input<string | undefined>(undefined);
  public readonly prefixIcon = input<string | undefined>(undefined);
  public readonly suffixIcon = input<string | undefined>(undefined);
  public readonly clearable = input<boolean>(false);
  public readonly autocomplete = input<string>('off');
  public readonly id = input<string | undefined>(undefined);

  protected readonly value = signal<string>('');
  protected readonly focused = signal(false);
  private readonly internalDisabled = signal(false);

  protected readonly disabledState = computed(() => this.disabled() || this.internalDisabled());
  protected readonly inputId = this.id() ?? `ds-text-field-${++dsTextFieldUid}`;

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
    const v = (event.target as HTMLInputElement).value;
    this.value.set(v);
    this.onChangeFn(v);
  }

  protected onFocus(): void {
    this.focused.set(true);
  }

  protected onBlur(): void {
    this.focused.set(false);
    this.onTouchedFn();
  }

  protected clear(): void {
    this.value.set('');
    this.onChangeFn('');
  }
}

import {
  ChangeDetectionStrategy,
  Component,
  computed,
  forwardRef,
  input,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface DsRadioOption {
  value: string | number;
  label: string;
  description?: string;
  disabled?: boolean;
}

/**
 * Design system radio group. Vertical or horizontal layout.
 */
@Component({
  selector: 'app-ds-radio-group',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ds-radio-group.component.html',
  styleUrl: './ds-radio-group.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsRadioGroupComponent),
      multi: true,
    },
  ],
})
export class DsRadioGroupComponent implements ControlValueAccessor {
  public readonly label = input<string | undefined>(undefined);
  public readonly options = input.required<DsRadioOption[]>();
  public readonly name = input.required<string>();
  public readonly disabled = input<boolean>(false);
  public readonly direction = input<'horizontal' | 'vertical'>('vertical');

  protected readonly value = signal<string | number | null>(null);
  private readonly internalDisabled = signal(false);

  protected readonly disabledState = computed(() => this.disabled() || this.internalDisabled());

  private onChangeFn: (v: string | number | null) => void = () => {};
  private onTouchedFn: () => void = () => {};

  public writeValue(v: string | number | null): void {
    this.value.set(v ?? null);
  }

  public registerOnChange(fn: (v: string | number | null) => void): void {
    this.onChangeFn = fn;
  }

  public registerOnTouched(fn: () => void): void {
    this.onTouchedFn = fn;
  }

  public setDisabledState(isDisabled: boolean): void {
    this.internalDisabled.set(isDisabled);
  }

  protected select(v: string | number): void {
    if (this.disabledState()) {
      return;
    }
    this.value.set(v);
    this.onChangeFn(v);
  }

  protected onBlur(): void {
    this.onTouchedFn();
  }
}

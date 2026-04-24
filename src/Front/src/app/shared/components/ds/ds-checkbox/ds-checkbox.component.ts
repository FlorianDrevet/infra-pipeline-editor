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
import { MatIconModule } from '@angular/material/icon';

/**
 * Design system checkbox. Square, brand blue when checked, supports indeterminate.
 */
@Component({
  selector: 'app-ds-checkbox',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './ds-checkbox.component.html',
  styleUrl: './ds-checkbox.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsCheckboxComponent),
      multi: true,
    },
  ],
})
export class DsCheckboxComponent implements ControlValueAccessor {
  public readonly label = input<string | undefined>(undefined);
  public readonly description = input<string | undefined>(undefined);
  public readonly disabled = input<boolean>(false);
  public readonly indeterminate = input<boolean>(false);

  protected readonly checked = signal<boolean>(false);
  private readonly internalDisabled = signal(false);

  protected readonly disabledState = computed(() => this.disabled() || this.internalDisabled());

  private onChangeFn: (v: boolean) => void = () => {};
  private onTouchedFn: () => void = () => {};

  public writeValue(v: boolean | null): void {
    this.checked.set(!!v);
  }

  public registerOnChange(fn: (v: boolean) => void): void {
    this.onChangeFn = fn;
  }

  public registerOnTouched(fn: () => void): void {
    this.onTouchedFn = fn;
  }

  public setDisabledState(isDisabled: boolean): void {
    this.internalDisabled.set(isDisabled);
  }

  protected onToggle(event: Event): void {
    const v = (event.target as HTMLInputElement).checked;
    this.checked.set(v);
    this.onChangeFn(v);
  }

  protected onBlur(): void {
    this.onTouchedFn();
  }
}

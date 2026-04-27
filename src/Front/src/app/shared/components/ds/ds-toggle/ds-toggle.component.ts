import {
  ChangeDetectionStrategy,
  Component,
  computed,
  forwardRef,
  input,
  signal,
} from '@angular/core';

import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

/**
 * Design system slide toggle. iOS-style switch with brand gradient when checked.
 */
@Component({
  selector: 'app-ds-toggle',
  standalone: true,
  imports: [],
  templateUrl: './ds-toggle.component.html',
  styleUrl: './ds-toggle.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsToggleComponent),
      multi: true,
    },
  ],
})
export class DsToggleComponent implements ControlValueAccessor {
  public readonly label = input<string | undefined>(undefined);
  public readonly description = input<string | undefined>(undefined);
  public readonly ariaLabel = input<string | undefined>(undefined);
  public readonly disabled = input<boolean>(false);
  public readonly labelPosition = input<'before' | 'after'>('after');

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

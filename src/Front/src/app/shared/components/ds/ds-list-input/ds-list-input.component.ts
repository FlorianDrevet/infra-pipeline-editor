import {
  ChangeDetectionStrategy,
  Component,
  computed,
  forwardRef,
  input,
  output,
  signal,
} from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';

import { DsButtonComponent } from '../ds-button/ds-button.component';
import { DsChipComponent } from '../ds-chip/ds-chip.component';
import { DsIpInputComponent } from '../ds-ip-input/ds-ip-input.component';
import { DsTextFieldComponent } from '../ds-text-field/ds-text-field.component';
import { DsListInputValidator } from './ds-list-input.types';

/**
 * Design system single-value list input. Manages a list of string items with
 * add/remove capabilities. Honors Reactive Forms via {@link ControlValueAccessor}.
 */
@Component({
  selector: 'app-ds-list-input',
  standalone: true,
  imports: [DsButtonComponent, DsChipComponent, DsIpInputComponent, DsTextFieldComponent, ReactiveFormsModule],
  templateUrl: './ds-list-input.component.html',
  styleUrl: './ds-list-input.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsListInputComponent),
      multi: true,
    },
  ],
})
export class DsListInputComponent implements ControlValueAccessor {
  public readonly label = input<string | undefined>(undefined);
  public readonly placeholder = input<string>('');
  public readonly helperText = input<string | undefined>(undefined);
  public readonly errorText = input<string | undefined>(undefined);
  public readonly disabled = input<boolean>(false);
  public readonly required = input<boolean>(false);
  public readonly maxItems = input<number | undefined>(undefined);
  public readonly addButtonLabel = input<string>('Add');
  public readonly validator = input<DsListInputValidator | undefined>(undefined);
  public readonly inputType = input<'text' | 'cidr' | 'ipv4'>('text');

  public readonly itemAdded = output<string>();
  public readonly itemRemoved = output<string>();
  public readonly itemsChange = output<string[]>();

  protected readonly items = signal<string[]>([]);
  protected readonly inputCtrl = new FormControl('', { nonNullable: true });
  protected readonly internalError = signal<string | undefined>(undefined);
  private readonly internalDisabled = signal<boolean>(false);

  protected readonly disabledState = computed(() => this.disabled() || this.internalDisabled());
  protected readonly resolvedError = computed(() => this.errorText() ?? this.internalError());
  protected readonly canAddMore = computed(() => {
    const max = this.maxItems();
    return max === undefined || this.items().length < max;
  });

  private onChangeFn: (value: string[]) => void = () => {};
  private onTouchedFn: () => void = () => {};

  public writeValue(value: string[] | null): void {
    this.items.set(value ?? []);
  }

  public registerOnChange(fn: (value: string[]) => void): void {
    this.onChangeFn = fn;
  }

  public registerOnTouched(fn: () => void): void {
    this.onTouchedFn = fn;
  }

  public setDisabledState(isDisabled: boolean): void {
    this.internalDisabled.set(isDisabled);
    if (isDisabled) {
      this.inputCtrl.disable();
    } else {
      this.inputCtrl.enable();
    }
  }

  protected onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.tryAdd();
    }
  }

  protected tryAdd(): void {
    if (this.disabledState() || !this.canAddMore()) return;

    const value = this.inputCtrl.value.trim();
    if (!value) return;

    if (this.items().includes(value)) {
      this.internalError.set('DS.LIST_INPUT.DUPLICATE');
      return;
    }

    const validatorFn = this.validator();
    if (validatorFn) {
      const result = validatorFn(value, this.items());
      if (result === false) return;
      if (typeof result === 'string') {
        this.internalError.set(result);
        return;
      }
    }

    const updated = [...this.items(), value];
    this.items.set(updated);
    this.inputCtrl.reset();
    this.internalError.set(undefined);

    this.onChangeFn(updated);
    this.onTouchedFn();
    this.itemAdded.emit(value);
    this.itemsChange.emit(updated);
  }

  protected removeItem(index: number): void {
    if (this.disabledState()) return;

    const removed = this.items()[index];
    const updated = this.items().filter((_, i) => i !== index);
    this.items.set(updated);

    this.onChangeFn(updated);
    this.onTouchedFn();
    this.itemRemoved.emit(removed);
    this.itemsChange.emit(updated);
  }
}

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
import { DsTextFieldComponent } from '../ds-text-field/ds-text-field.component';
import { DsKeyValueItem, DsKeyValueValidator } from './ds-key-value-input.types';

/**
 * Design system key-value pair input. Manages a list of key/value pairs with
 * add/remove capabilities. Honors Reactive Forms via {@link ControlValueAccessor}.
 */
@Component({
  selector: 'app-ds-key-value-input',
  standalone: true,
  imports: [DsButtonComponent, DsChipComponent, DsTextFieldComponent, ReactiveFormsModule],
  templateUrl: './ds-key-value-input.component.html',
  styleUrl: './ds-key-value-input.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsKeyValueInputComponent),
      multi: true,
    },
  ],
})
export class DsKeyValueInputComponent implements ControlValueAccessor {
  public readonly label = input<string | undefined>(undefined);
  public readonly keyPlaceholder = input<string>('');
  public readonly valuePlaceholder = input<string>('');
  public readonly helperText = input<string | undefined>(undefined);
  public readonly errorText = input<string | undefined>(undefined);
  public readonly disabled = input<boolean>(false);
  public readonly required = input<boolean>(false);
  public readonly maxItems = input<number | undefined>(undefined);
  public readonly validator = input<DsKeyValueValidator | undefined>(undefined);
  public readonly addButtonLabel = input<string>('Add');
  public readonly keyLabel = input<string>('');
  public readonly valueLabel = input<string>('');
  public readonly duplicateKeyError = input<string>('');

  public readonly itemAdded = output<DsKeyValueItem>();
  public readonly itemRemoved = output<DsKeyValueItem>();
  public readonly itemsChange = output<DsKeyValueItem[]>();

  protected readonly items = signal<DsKeyValueItem[]>([]);
  protected readonly keyCtrl = new FormControl('', { nonNullable: true });
  protected readonly valueCtrl = new FormControl('', { nonNullable: true });
  protected readonly internalError = signal<string | undefined>(undefined);
  private readonly internalDisabled = signal<boolean>(false);

  protected readonly disabledState = computed(() => this.disabled() || this.internalDisabled());
  protected readonly resolvedError = computed(() => this.errorText() ?? this.internalError());
  protected readonly canAddMore = computed(() => {
    const max = this.maxItems();
    return max === undefined || this.items().length < max;
  });

  private onChangeFn: (value: DsKeyValueItem[]) => void = () => {};
  private onTouchedFn: () => void = () => {};

  public writeValue(value: DsKeyValueItem[] | null): void {
    this.items.set(value ?? []);
  }

  public registerOnChange(fn: (value: DsKeyValueItem[]) => void): void {
    this.onChangeFn = fn;
  }

  public registerOnTouched(fn: () => void): void {
    this.onTouchedFn = fn;
  }

  public setDisabledState(isDisabled: boolean): void {
    this.internalDisabled.set(isDisabled);
    if (isDisabled) {
      this.keyCtrl.disable();
      this.valueCtrl.disable();
    } else {
      this.keyCtrl.enable();
      this.valueCtrl.enable();
    }
  }

  protected onValueKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.tryAdd();
    }
  }

  protected tryAdd(): void {
    if (this.disabledState() || !this.canAddMore()) return;

    const key = this.keyCtrl.value.trim();
    const value = this.valueCtrl.value.trim();

    if (!key || !value) {
      return;
    }

    if (this.items().some(item => item.key === key)) {
      this.internalError.set(this.duplicateKeyError() || 'DS.KEY_VALUE_INPUT.DUPLICATE_KEY');
      return;
    }

    const validatorFn = this.validator();
    if (validatorFn) {
      const result = validatorFn(key, value, this.items());
      if (result === false) return;
      if (typeof result === 'string') {
        this.internalError.set(result);
        return;
      }
    }

    const newItem: DsKeyValueItem = { key, value };
    const updated = [...this.items(), newItem];
    this.items.set(updated);
    this.keyCtrl.reset();
    this.valueCtrl.reset();
    this.internalError.set(undefined);

    this.onChangeFn(updated);
    this.onTouchedFn();
    this.itemAdded.emit(newItem);
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

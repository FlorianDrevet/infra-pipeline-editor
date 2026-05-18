import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  ViewEncapsulation,
  computed,
  forwardRef,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent,
  MatAutocompleteTrigger,
} from '@angular/material/autocomplete';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

let dsAutocompleteUid = 0;

export interface DsAutocompleteOption<TValue = string> {
  value: TValue;
  label: string;
  icon?: string;
  disabled?: boolean;
  description?: string;
}

/**
 * Design system autocomplete field. Keeps a DS-styled input while delegating panel
 * positioning and keyboard navigation to Angular Material autocomplete.
 */
@Component({
  selector: 'app-ds-autocomplete',
  standalone: true,
  imports: [MatAutocompleteModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './ds-autocomplete.component.html',
  styleUrl: './ds-autocomplete.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsAutocompleteComponent),
      multi: true,
    },
  ],
})
export class DsAutocompleteComponent implements ControlValueAccessor {
  public readonly options = input.required<ReadonlyArray<DsAutocompleteOption<unknown>>>();
  public readonly label = input<string | undefined>(undefined);
  public readonly placeholder = input<string>('');
  public readonly hint = input<string | undefined>(undefined);
  public readonly error = input<string | undefined>(undefined);
  public readonly disabled = input<boolean>(false);
  public readonly required = input<boolean>(false);
  public readonly prefixIcon = input<string | undefined>(undefined);
  public readonly clearable = input<boolean>(false);
  public readonly loading = input<boolean>(false);
  public readonly loadingStateLabel = input<string>('Loading...');
  public readonly emptyStateLabel = input<string>('No results');
  public readonly autocomplete = input<string>('off');
  public readonly id = input<string | undefined>(undefined);

  public readonly optionSelected = output<DsAutocompleteOption<unknown>>();
  public readonly searchChanged = output<string>();
  public readonly inputFocused = output<void>();

  protected readonly value = signal('');
  protected readonly focused = signal(false);
  private readonly internalDisabled = signal(false);
  private readonly generatedId = `ds-autocomplete-${++dsAutocompleteUid}`;
  private readonly inputRef = viewChild<ElementRef<HTMLInputElement>>('inputRef');
  private readonly trigger = viewChild(MatAutocompleteTrigger);

  protected readonly disabledState = computed(() => this.disabled() || this.internalDisabled());
  protected readonly inputId = computed(() => this.id() ?? this.generatedId);
  protected readonly hasOptions = computed(() => this.options().length > 0);

  private onChangeFn: (value: string) => void = () => {};
  private onTouchedFn: () => void = () => {};

  public writeValue(value: string | null): void {
    this.value.set(value ?? '');
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

  protected onInput(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    const nextValue = inputElement.value;

    this.value.set(nextValue);
    this.onChangeFn(nextValue);
    this.searchChanged.emit(nextValue);

    if (!this.disabledState()) {
      this.trigger()?.openPanel();
    }
  }

  protected onFocus(): void {
    if (this.disabledState()) {
      return;
    }

    this.focused.set(true);
    this.inputFocused.emit();
    this.trigger()?.openPanel();
  }

  protected onBlur(): void {
    queueMicrotask(() => {
      if (this.trigger()?.panelOpen) {
        return;
      }

      this.focused.set(false);
      this.onTouchedFn();
    });
  }

  protected onClosed(): void {
    this.focused.set(false);
    this.onTouchedFn();
  }

  protected onOptionSelected(event: MatAutocompleteSelectedEvent): void {
    const option = event.option.value as DsAutocompleteOption<unknown>;

    this.value.set(option.label);
    this.onChangeFn(option.label);
    this.optionSelected.emit(option);
  }

  protected clear(): void {
    this.value.set('');
    this.onChangeFn('');
    this.searchChanged.emit('');

    const inputElement = this.inputRef()?.nativeElement;
    inputElement?.focus();
    this.trigger()?.openPanel();
  }

  protected displayOptionLabel(option: DsAutocompleteOption<unknown> | string | null): string {
    if (typeof option === 'string') {
      return option;
    }

    return option?.label ?? '';
  }
}
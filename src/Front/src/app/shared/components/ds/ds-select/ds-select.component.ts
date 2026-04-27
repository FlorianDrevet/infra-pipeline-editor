import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  computed,
  forwardRef,
  input,
  signal,
  viewChild,
} from '@angular/core';

import { ConnectedPosition, OverlayModule } from '@angular/cdk/overlay';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

export interface DsSelectOption {
  value: string | number | null;
  label: string;
  icon?: string;
  disabled?: boolean;
  description?: string;
}

/**
 * Design system single-select. Custom dropdown panel with optional search and icons.
 */
@Component({
  selector: 'app-ds-select',
  standalone: true,
  imports: [MatIconModule, OverlayModule],
  templateUrl: './ds-select.component.html',
  styleUrl: './ds-select.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsSelectComponent),
      multi: true,
    },
  ],
})
export class DsSelectComponent implements ControlValueAccessor {
  public readonly label = input<string | undefined>(undefined);
  public readonly placeholder = input<string>('Select…');
  public readonly options = input.required<DsSelectOption[]>();
  public readonly disabled = input<boolean>(false);
  public readonly required = input<boolean>(false);
  public readonly hint = input<string | undefined>(undefined);
  public readonly error = input<string | undefined>(undefined);
  public readonly clearable = input<boolean>(false);
  public readonly searchable = input<boolean>(false);

  protected readonly value = signal<string | number | null>(null);
  protected readonly isOpen = signal(false);
  protected readonly searchTerm = signal('');
  private readonly internalDisabled = signal(false);
  private readonly triggerRef = viewChild<ElementRef<HTMLButtonElement>>('trigger');

  protected readonly disabledState = computed(() => this.disabled() || this.internalDisabled());
  protected readonly overlayWidth = computed(() => this.triggerRef()?.nativeElement.getBoundingClientRect().width ?? 0);
  protected readonly overlayPositions: ConnectedPosition[] = [
    {
      originX: 'start',
      originY: 'bottom',
      overlayX: 'start',
      overlayY: 'top',
      offsetY: 6,
    },
    {
      originX: 'start',
      originY: 'top',
      overlayX: 'start',
      overlayY: 'bottom',
      offsetY: -6,
    },
  ];

  protected readonly selectedOption = computed(() =>
    this.options().find((o) => o.value === this.value()),
  );

  protected readonly filteredOptions = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!this.searchable() || !term) {
      return this.options();
    }
    return this.options().filter((o) => o.label.toLowerCase().includes(term));
  });

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

  protected toggleOpen(): void {
    if (this.disabledState()) {
      return;
    }
    this.isOpen.update((v) => !v);
    if (!this.isOpen()) {
      this.searchTerm.set('');
    }
  }

  protected close(): void {
    if (this.isOpen()) {
      this.isOpen.set(false);
      this.searchTerm.set('');
      this.onTouchedFn();
    }
  }

  protected onSearch(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
  }

  protected select(opt: DsSelectOption): void {
    if (opt.disabled) {
      return;
    }
    this.value.set(opt.value);
    this.onChangeFn(opt.value);
    this.close();
  }

  protected clear(): void {
    this.value.set(null);
    this.onChangeFn(null);
  }

  protected onBlur(): void {
    // Touched is emitted on close.
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    this.close();
  }
}

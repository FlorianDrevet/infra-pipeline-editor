import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  effect,
  forwardRef,
  input,
  signal,
  viewChildren,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

import { DsSegmentedOption, DsSegmentedSize } from './ds-segmented-control.types';

/**
 * Design system segmented control (radio-group of buttons).
 *
 * Implements {@link ControlValueAccessor} so it can be used with template-driven
 * (`ngModel`) and reactive forms (`FormControl`). Keyboard navigation:
 * ArrowLeft / ArrowRight cycle through enabled options.
 */
@Component({
  selector: 'app-ds-segmented-control',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-segmented-control.component.html',
  styleUrl: './ds-segmented-control.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsSegmentedControlComponent),
      multi: true,
    },
  ],
})
export class DsSegmentedControlComponent implements ControlValueAccessor {
  public readonly options = input.required<readonly DsSegmentedOption[]>();
  public readonly size = input<DsSegmentedSize>('md');
  public readonly ariaLabel = input<string | undefined>(undefined);

  protected readonly value = signal<string | null>(null);
  protected readonly disabledByForm = signal<boolean>(false);

  protected readonly optionButtons = viewChildren<ElementRef<HTMLButtonElement>>('optionButton');

  private readonly focusIndex = signal<number | null>(null);

  protected readonly resolvedValue = computed<string | null>(() => {
    const explicit = this.value();
    if (explicit !== null && this.options().some((option) => option.value === explicit && !option.disabled)) {
      return explicit;
    }
    return null;
  });

  private onChange: (value: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  public constructor() {
    effect(() => {
      const focusAt = this.focusIndex();
      if (focusAt === null) {
        return;
      }
      const buttons = this.optionButtons();
      buttons[focusAt]?.nativeElement.focus();
      this.focusIndex.set(null);
    });
  }

  public writeValue(value: string | null | undefined): void {
    this.value.set(value ?? null);
  }

  public registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  public registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  public setDisabledState(isDisabled: boolean): void {
    this.disabledByForm.set(isDisabled);
  }

  protected isSelected(option: DsSegmentedOption): boolean {
    return option.value === this.resolvedValue();
  }

  protected isDisabled(option: DsSegmentedOption): boolean {
    return this.disabledByForm() || !!option.disabled;
  }

  protected onSelect(option: DsSegmentedOption): void {
    if (this.isDisabled(option) || option.value === this.value()) {
      return;
    }
    this.value.set(option.value);
    this.onChange(option.value);
    this.onTouched();
  }

  protected onKeyDown(event: KeyboardEvent, currentIndex: number): void {
    let direction: 1 | -1 | null = null;
    if (event.key === 'ArrowRight') {
      direction = 1;
    } else if (event.key === 'ArrowLeft') {
      direction = -1;
    } else {
      return;
    }

    const nextIndex = this.findNextEnabled(currentIndex, direction);
    if (nextIndex === null || nextIndex === currentIndex) {
      return;
    }

    event.preventDefault();
    const target = this.options()[nextIndex];
    this.value.set(target.value);
    this.onChange(target.value);
    this.onTouched();
    this.focusIndex.set(nextIndex);
  }

  private findNextEnabled(fromIndex: number, direction: 1 | -1): number | null {
    const options = this.options();
    const length = options.length;
    for (let step = 1; step <= length; step++) {
      const candidate = fromIndex + direction * step;
      if (candidate < 0 || candidate >= length) {
        continue;
      }
      if (!options[candidate].disabled) {
        return candidate;
      }
    }
    return null;
  }
}

import {
  ChangeDetectionStrategy,
  Component,
  computed,
  forwardRef,
  input,
  output,
  signal,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { DsChipComponent } from '../ds-chip/ds-chip.component';
import { DsTagInputItem, DsTagInputValidator } from './ds-tag-input.types';

let dsTagInputUid = 0;

const DEFAULT_SEPARATOR_REGEX = /[,;\n]/;

/**
 * Design system multi-value tag input. Wraps a chip row + a native input. Honors
 * Reactive Forms via {@link ControlValueAccessor}. Supports adding tags via
 * Enter, comma, or blur, and removing via the chip remove icon, Backspace
 * (when the input is empty), or Delete on a focused chip.
 */
@Component({
  selector: 'app-ds-tag-input',
  standalone: true,
  imports: [DsChipComponent],
  templateUrl: './ds-tag-input.component.html',
  styleUrl: './ds-tag-input.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsTagInputComponent),
      multi: true,
    },
  ],
})
export class DsTagInputComponent implements ControlValueAccessor {
  public readonly label = input<string | undefined>(undefined);
  public readonly placeholder = input<string>('');
  public readonly helperText = input<string | undefined>(undefined);
  public readonly errorText = input<string | undefined>(undefined);
  public readonly disabled = input<boolean>(false);
  public readonly required = input<boolean>(false);
  public readonly addOnEnter = input<boolean>(true);
  public readonly addOnComma = input<boolean>(true);
  public readonly addOnBlur = input<boolean>(false);
  public readonly maxTags = input<number | undefined>(undefined);
  public readonly validator = input<DsTagInputValidator | undefined>(undefined);
  public readonly id = input<string | undefined>(undefined);

  public readonly tagAdded = output<DsTagInputItem>();
  public readonly tagRemoved = output<DsTagInputItem>();
  public readonly tagsChange = output<DsTagInputItem[]>();

  protected readonly tags = signal<DsTagInputItem[]>([]);
  protected readonly draft = signal<string>('');
  protected readonly focused = signal<boolean>(false);
  protected readonly internalError = signal<string | undefined>(undefined);
  private readonly internalDisabled = signal<boolean>(false);

  protected readonly disabledState = computed(() => this.disabled() || this.internalDisabled());
  protected readonly inputId = this.id() ?? `ds-tag-input-${++dsTagInputUid}`;
  protected readonly helperId = `${this.inputId}-helper`;
  protected readonly errorId = `${this.inputId}-error`;
  protected readonly resolvedError = computed(() => this.errorText() ?? this.internalError());
  protected readonly describedBy = computed(() => {
    const ids: string[] = [];
    if (this.resolvedError()) {
      ids.push(this.errorId);
    } else if (this.helperText()) {
      ids.push(this.helperId);
    }
    return ids.length > 0 ? ids.join(' ') : null;
  });
  protected readonly canAddMore = computed(() => {
    const max = this.maxTags();
    return max === undefined || this.tags().length < max;
  });

  private onChangeFn: (value: DsTagInputItem[]) => void = () => {};
  private onTouchedFn: () => void = () => {};

  public writeValue(value: DsTagInputItem[] | null): void {
    this.tags.set(value ?? []);
  }

  public registerOnChange(fn: (value: DsTagInputItem[]) => void): void {
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

    if (this.addOnComma() && DEFAULT_SEPARATOR_REGEX.test(raw)) {
      const parts = raw.split(DEFAULT_SEPARATOR_REGEX);
      const remaining = parts.pop() ?? '';
      for (const part of parts) {
        this.tryAdd(part);
      }
      this.draft.set(remaining);
      input.value = remaining;
      return;
    }

    this.draft.set(raw);
    this.internalError.set(undefined);
  }

  protected onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && this.addOnEnter()) {
      event.preventDefault();
      this.tryAdd(this.draft());
      return;
    }
    if (event.key === 'Backspace' && this.draft().length === 0 && this.tags().length > 0) {
      event.preventDefault();
      this.removeAt(this.tags().length - 1);
    }
  }

  protected onFocus(): void {
    this.focused.set(true);
  }

  protected onBlur(): void {
    this.focused.set(false);
    if (this.addOnBlur()) {
      this.tryAdd(this.draft());
    }
    this.onTouchedFn();
  }

  protected onChipKeyDown(event: KeyboardEvent, index: number): void {
    if (event.key === 'Delete' || event.key === 'Backspace') {
      event.preventDefault();
      this.removeAt(index);
    }
  }

  protected onChipRemove(index: number): void {
    this.removeAt(index);
  }

  private tryAdd(rawValue: string): void {
    if (this.disabledState()) {
      return;
    }
    const trimmed = rawValue.trim();
    if (trimmed.length === 0) {
      return;
    }
    if (!this.canAddMore()) {
      return;
    }
    const existing = this.tags();
    if (existing.some((tag) => tag.value === trimmed)) {
      this.draft.set('');
      return;
    }
    const validator = this.validator();
    if (validator) {
      const verdict = validator(trimmed, existing);
      if (verdict === false) {
        return;
      }
      if (typeof verdict === 'string') {
        this.internalError.set(verdict);
        return;
      }
    }
    const item: DsTagInputItem = { value: trimmed };
    const next = [...existing, item];
    this.tags.set(next);
    this.draft.set('');
    this.internalError.set(undefined);
    this.onChangeFn(next);
    this.tagAdded.emit(item);
    this.tagsChange.emit(next);
  }

  private removeAt(index: number): void {
    if (this.disabledState()) {
      return;
    }
    const current = this.tags();
    const target = current[index];
    if (!target) {
      return;
    }
    if (target.removable === false) {
      return;
    }
    const next = current.filter((_, i) => i !== index);
    this.tags.set(next);
    this.onChangeFn(next);
    this.tagRemoved.emit(target);
    this.tagsChange.emit(next);
  }
}

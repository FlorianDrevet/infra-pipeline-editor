import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  effect,
  forwardRef,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

let dsTextareaUid = 0;

/**
 * Design system textarea. Multi-line input with optional auto-resize and character counter.
 */
@Component({
  selector: 'app-ds-textarea',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ds-textarea.component.html',
  styleUrl: './ds-textarea.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsTextareaComponent),
      multi: true,
    },
  ],
})
export class DsTextareaComponent implements ControlValueAccessor, AfterViewInit {
  public readonly label = input<string | undefined>(undefined);
  public readonly placeholder = input<string>('');
  public readonly hint = input<string | undefined>(undefined);
  public readonly error = input<string | undefined>(undefined);
  public readonly disabled = input<boolean>(false);
  public readonly required = input<boolean>(false);
  public readonly autocomplete = input<string>('off');
  public readonly id = input<string | undefined>(undefined);
  public readonly rows = input<number>(4);
  public readonly maxLength = input<number | undefined>(undefined);
  public readonly autoResize = input<boolean>(false);

  protected readonly value = signal<string>('');
  protected readonly focused = signal(false);
  private readonly internalDisabled = signal(false);

  protected readonly disabledState = computed(() => this.disabled() || this.internalDisabled());
  protected readonly inputId = this.id() ?? `ds-textarea-${++dsTextareaUid}`;

  protected readonly textareaEl = viewChild<ElementRef<HTMLTextAreaElement>>('textareaEl');

  private onChangeFn: (v: string) => void = () => {};
  private onTouchedFn: () => void = () => {};

  public constructor() {
    effect(() => {
      // Re-evaluate auto-resize whenever value changes.
      this.value();
      if (this.autoResize()) {
        queueMicrotask(() => this.adjustHeight());
      }
    });
  }

  public ngAfterViewInit(): void {
    if (this.autoResize()) {
      this.adjustHeight();
    }
  }

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
    const v = (event.target as HTMLTextAreaElement).value;
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

  private adjustHeight(): void {
    const el = this.textareaEl()?.nativeElement;
    if (!el) {
      return;
    }
    el.style.height = 'auto';
    el.style.height = `${el.scrollHeight}px`;
  }
}

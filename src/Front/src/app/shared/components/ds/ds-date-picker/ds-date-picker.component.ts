import {
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  forwardRef,
  HostListener,
  input,
  signal,
  viewChild,
} from '@angular/core';

import { ConnectedPosition, OverlayModule } from '@angular/cdk/overlay';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

export interface CalendarDay {
  date: Date;
  day: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  isSelected: boolean;
  isDisabled: boolean;
}

let dsDatePickerUid = 0;

/**
 * Design system date picker. Custom calendar dropdown with month navigation.
 * Supports Reactive Forms (formControl), template-driven (ngModel) and two-way value binding.
 * Value format: ISO date string 'YYYY-MM-DD' or empty string.
 */
@Component({
  selector: 'app-ds-date-picker',
  standalone: true,
  imports: [MatIconModule, OverlayModule],
  templateUrl: './ds-date-picker.component.html',
  styleUrl: './ds-date-picker.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsDatePickerComponent),
      multi: true,
    },
  ],
})
export class DsDatePickerComponent implements ControlValueAccessor {
  public readonly label = input<string | undefined>(undefined);
  public readonly placeholder = input<string>('');
  public readonly hint = input<string | undefined>(undefined);
  public readonly error = input<string | undefined>(undefined);
  public readonly disabled = input<boolean>(false);
  public readonly required = input<boolean>(false);
  public readonly min = input<string | undefined>(undefined);
  public readonly max = input<string | undefined>(undefined);
  public readonly clearable = input<boolean>(false);

  protected readonly value = signal<string>('');
  protected readonly isOpen = signal(false);
  protected readonly viewDate = signal(new Date());
  private readonly internalDisabled = signal(false);
  private readonly triggerRef = viewChild<ElementRef<HTMLButtonElement>>('trigger');

  protected readonly inputId = `ds-date-picker-${++dsDatePickerUid}`;

  protected readonly disabledState = computed(() => this.disabled() || this.internalDisabled());
  protected readonly overlayWidth = computed(() => {
    const w = this.triggerRef()?.nativeElement.getBoundingClientRect().width ?? 0;
    return Math.max(w, 280);
  });

  protected readonly overlayPositions: ConnectedPosition[] = [
    { originX: 'start', originY: 'bottom', overlayX: 'start', overlayY: 'top', offsetY: 6 },
    { originX: 'start', originY: 'top', overlayX: 'start', overlayY: 'bottom', offsetY: -6 },
  ];

  protected readonly displayValue = computed(() => {
    const iso = this.value();
    if (!iso) return '';
    const parts = iso.split('-');
    const d = new Date(+parts[0], +parts[1] - 1, +parts[2]);
    if (isNaN(d.getTime())) return '';
    return new Intl.DateTimeFormat(navigator.language, {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    }).format(d);
  });

  protected readonly monthLabel = computed(() => {
    const d = this.viewDate();
    return new Intl.DateTimeFormat(navigator.language, {
      month: 'long',
      year: 'numeric',
    }).format(d);
  });

  protected readonly weekdays = computed(() => {
    const base = new Date(2024, 0, 1); // Monday
    const days: string[] = [];
    for (let i = 0; i < 7; i++) {
      const d = new Date(base);
      d.setDate(base.getDate() + i);
      days.push(
        new Intl.DateTimeFormat(navigator.language, { weekday: 'short' }).format(d),
      );
    }
    return days;
  });

  protected readonly calendarDays = computed(() => {
    const vd = this.viewDate();
    const year = vd.getFullYear();
    const month = vd.getMonth();
    const today = new Date();
    const selectedIso = this.value();
    const minIso = this.min();
    const maxIso = this.max();

    const firstOfMonth = new Date(year, month, 1);
    let dayOfWeek = firstOfMonth.getDay();
    // Convert Sunday=0 to Monday-start: Mon=0 ... Sun=6
    dayOfWeek = dayOfWeek === 0 ? 6 : dayOfWeek - 1;

    const start = new Date(firstOfMonth);
    start.setDate(start.getDate() - dayOfWeek);

    const days: CalendarDay[] = [];
    for (let i = 0; i < 42; i++) {
      const date = new Date(start);
      date.setDate(start.getDate() + i);
      const iso = this.toIso(date);

      days.push({
        date,
        day: date.getDate(),
        isCurrentMonth: date.getMonth() === month,
        isToday:
          date.getDate() === today.getDate() &&
          date.getMonth() === today.getMonth() &&
          date.getFullYear() === today.getFullYear(),
        isSelected: iso === selectedIso,
        isDisabled: (!!minIso && iso < minIso) || (!!maxIso && iso > maxIso),
      });
    }
    return days;
  });

  private onChangeFn: (v: string) => void = () => {};
  private onTouchedFn: () => void = () => {};

  public writeValue(v: string | null): void {
    this.value.set(v ?? '');
    if (v) {
      const parts = v.split('-');
      this.viewDate.set(new Date(+parts[0], +parts[1] - 1, 1));
    }
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

  protected toggleOpen(): void {
    if (this.disabledState()) return;
    if (!this.isOpen()) {
      // Reset viewDate to selected value or today
      const v = this.value();
      if (v) {
        const parts = v.split('-');
        this.viewDate.set(new Date(+parts[0], +parts[1] - 1, 1));
      } else {
        const now = new Date();
        this.viewDate.set(new Date(now.getFullYear(), now.getMonth(), 1));
      }
    }
    this.isOpen.update((o) => !o);
  }

  protected close(): void {
    if (this.isOpen()) {
      this.isOpen.set(false);
      this.onTouchedFn();
    }
  }

  protected prevMonth(): void {
    const d = this.viewDate();
    this.viewDate.set(new Date(d.getFullYear(), d.getMonth() - 1, 1));
  }

  protected nextMonth(): void {
    const d = this.viewDate();
    this.viewDate.set(new Date(d.getFullYear(), d.getMonth() + 1, 1));
  }

  protected selectDay(day: CalendarDay): void {
    if (day.isDisabled || !day.isCurrentMonth) return;
    const iso = this.toIso(day.date);
    this.value.set(iso);
    this.onChangeFn(iso);
    this.close();
  }

  protected clear(event: Event): void {
    event.stopPropagation();
    this.value.set('');
    this.onChangeFn('');
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    this.close();
  }

  private toIso(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }
}

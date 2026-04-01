import { Component, input, output, signal, computed, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { OverlayModule, CdkOverlayOrigin, CdkConnectedOverlay } from '@angular/cdk/overlay';

export interface CompactSelectOption {
  value: string;
  label: string;
  icon?: string;
}

@Component({
  selector: 'app-compact-select',
  standalone: true,
  imports: [CommonModule, MatIconModule, OverlayModule],
  templateUrl: './compact-select.component.html',
  styleUrl: './compact-select.component.scss',
})
export class CompactSelectComponent {
  readonly options = input<CompactSelectOption[]>([]);
  readonly value = input<string | null>(null);
  readonly placeholder = input('Select…');
  readonly label = input('');
  readonly disabled = input(false);
  readonly emptyText = input('No options available');

  readonly valueChange = output<string | null>();

  protected readonly isOpen = signal(false);

  protected readonly selectedOption = computed(() => {
    const v = this.value();
    if (!v) return null;
    return this.options().find(o => o.value === v) ?? null;
  });

  protected readonly displayText = computed(() => {
    const sel = this.selectedOption();
    return sel ? sel.label : this.placeholder();
  });

  protected toggle(): void {
    if (this.disabled()) return;
    this.isOpen.update(v => !v);
  }

  protected close(): void {
    this.isOpen.set(false);
  }

  protected select(option: CompactSelectOption): void {
    this.valueChange.emit(option.value);
    this.close();
  }
}

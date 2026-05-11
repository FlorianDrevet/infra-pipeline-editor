import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';

import { MatIconModule } from '@angular/material/icon';

import { DsAlertSeverity } from './ds-alert.types';

type ResolvedSeverity = 'info' | 'success' | 'warning' | 'danger';

const DEFAULT_ICONS: Record<ResolvedSeverity, string> = {
  info: 'info',
  success: 'check_circle',
  warning: 'warning',
  danger: 'error',
};

/**
 * Design system inline alert. Supports four severities, optional title,
 * dismissible action. Legacy `error` severity is mapped to `danger`.
 */
@Component({
  selector: 'app-ds-alert',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-alert.component.html',
  styleUrl: './ds-alert.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsAlertComponent {
  public readonly severity = input<DsAlertSeverity>('info');
  public readonly title = input<string | undefined>(undefined);
  public readonly dismissible = input<boolean>(false);
  public readonly icon = input<string | undefined>(undefined);

  public readonly dismissed = output<void>();

  protected readonly visible = signal(true);
  protected readonly resolvedSeverity = computed<ResolvedSeverity>(() =>
    this.severity() === 'error' ? 'danger' : (this.severity() as ResolvedSeverity),
  );
  protected readonly resolvedIcon = computed(() => this.icon() ?? DEFAULT_ICONS[this.resolvedSeverity()]);

  protected onDismiss(): void {
    this.visible.set(false);
    this.dismissed.emit();
  }
}

import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

type AlertSeverity = 'info' | 'success' | 'warning' | 'error';

const DEFAULT_ICONS: Record<AlertSeverity, string> = {
  info: 'info',
  success: 'check_circle',
  warning: 'warning',
  error: 'error',
};

/**
 * Design system inline alert. Supports four severities, optional title, dismissible action.
 */
@Component({
  selector: 'app-ds-alert',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './ds-alert.component.html',
  styleUrl: './ds-alert.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsAlertComponent {
  public readonly severity = input<AlertSeverity>('info');
  public readonly title = input<string | undefined>(undefined);
  public readonly dismissible = input<boolean>(false);
  public readonly icon = input<string | undefined>(undefined);

  public readonly dismissed = output<void>();

  protected readonly visible = signal(true);
  protected readonly resolvedIcon = computed(() => this.icon() ?? DEFAULT_ICONS[this.severity()]);

  protected onDismiss(): void {
    this.visible.set(false);
    this.dismissed.emit();
  }
}

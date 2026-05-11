import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';

import { MatIconModule } from '@angular/material/icon';

import { DsIconButtonComponent } from '../ds-icon-button/ds-icon-button.component';
import { DsBannerVariant } from './ds-banner.types';

const BANNER_ICONS: Record<DsBannerVariant, string> = {
  info: 'info',
  success: 'check_circle',
  warning: 'warning',
  danger: 'error',
};

/**
 * Design system page-level banner. Full-width strip with leading colored
 * indicator, icon, title, description and optional `[actions]` slot. Use for
 * persistent contextual messages (deprecation notice, sync status…). For
 * inline alerts inside cards prefer {@link DsAlertComponent}.
 */
@Component({
  selector: 'app-ds-banner',
  standalone: true,
  imports: [MatIconModule, DsIconButtonComponent],
  templateUrl: './ds-banner.component.html',
  styleUrl: './ds-banner.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsBannerComponent {
  public readonly variant = input<DsBannerVariant>('info');
  public readonly title = input<string | undefined>(undefined);
  public readonly description = input<string | undefined>(undefined);
  public readonly icon = input<string | undefined>(undefined);
  public readonly dismissible = input<boolean>(false);

  public readonly dismissed = output<void>();

  protected readonly visible = signal(true);
  protected readonly resolvedIcon = computed(() => this.icon() ?? BANNER_ICONS[this.variant()]);
  protected readonly role = computed(() => (this.variant() === 'danger' ? 'alert' : 'status'));

  protected onDismiss(): void {
    this.visible.set(false);
    this.dismissed.emit();
  }
}

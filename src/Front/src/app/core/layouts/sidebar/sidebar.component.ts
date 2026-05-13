import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import { DsIconButtonComponent } from '../../../shared/components/ds/ds-icon-button/ds-icon-button.component';
import { SidebarStateService } from './sidebar-state.service';
import { SidebarContextService } from './sidebar-context.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule, TranslateModule, DsIconButtonComponent],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.sidebar--collapsed]': 'collapsed()',
    '[style.width]': 'width()',
  },
})
export class SidebarComponent {
  private readonly state = inject(SidebarStateService);
  private readonly context = inject(SidebarContextService);

  protected readonly collapsed = this.state.collapsed;
  protected readonly width = this.state.width;
  protected readonly contextState = this.context.contextState;
  protected readonly mode = this.context.mode;
  protected readonly favoriteIds = this.context.favoriteIds;
  protected readonly recentItems = this.context.recentItems;

  protected readonly toggleIcon = computed(() => (this.collapsed() ? 'chevron_right' : 'chevron_left'));
  protected readonly toggleAriaLabelKey = computed(() =>
    this.collapsed() ? 'SIDEBAR.EXPAND_ARIA' : 'SIDEBAR.COLLAPSE_ARIA'
  );

  protected readonly defineItems = computed(() =>
    this.contextState().items.filter((i) => i.section === 'define')
  );
  protected readonly generateItems = computed(() =>
    this.contextState().items.filter((i) => i.section === 'generate')
  );
  protected readonly manageItems = computed(() =>
    this.contextState().items.filter((i) => i.section === 'manage')
  );
  protected readonly globalItems = computed(() =>
    this.contextState().items.filter((i) => !i.section)
  );

  protected onToggle(): void {
    this.state.toggle();
  }
}

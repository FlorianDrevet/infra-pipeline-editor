import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import { DsIconButtonComponent } from '../../../shared/components/ds/ds-icon-button/ds-icon-button.component';
import { SidebarStateService } from './sidebar-state.service';
import { SidebarNavItem } from './sidebar.types';

const NavItems: readonly SidebarNavItem[] = [
  { id: 'home', icon: 'home', labelKey: 'SIDEBAR.HOME', routerLink: '/', exact: true },
  { id: 'projects', icon: 'folder', labelKey: 'SIDEBAR.PROJECTS', routerLink: '/projects', exact: false },
  { id: 'settings', icon: 'settings', labelKey: 'SIDEBAR.SETTINGS', routerLink: '/settings', exact: false },
  // NOTE: route `/docs` is currently a placeholder; pending confirmation it may become an external link.
  { id: 'docs', icon: 'description', labelKey: 'SIDEBAR.DOCS', routerLink: '/docs', exact: false },
];

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

  protected readonly items = NavItems;
  protected readonly collapsed = this.state.collapsed;
  protected readonly width = this.state.width;

  protected readonly toggleIcon = computed(() => (this.collapsed() ? 'chevron_right' : 'chevron_left'));
  protected readonly toggleAriaLabelKey = computed(() =>
    this.collapsed() ? 'SIDEBAR.EXPAND_ARIA' : 'SIDEBAR.COLLAPSE_ARIA'
  );

  protected onToggle(): void {
    this.state.toggle();
  }
}

import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import { SidebarContextService } from './sidebar-context.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule, TranslateModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  private readonly context = inject(SidebarContextService);

  protected readonly contextState = this.context.contextState;
  protected readonly mode = this.context.mode;
  protected readonly favoriteIds = this.context.favoriteIds;
  protected readonly recentItems = this.context.recentItems;

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
}

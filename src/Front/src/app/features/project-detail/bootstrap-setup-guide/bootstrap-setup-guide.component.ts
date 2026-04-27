import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

/**
 * Step-by-step guide explaining how to push, run and grant permissions for the
 * generated bootstrap pipeline in Azure DevOps. Rendered inside the bootstrap
 * tabs of both the legacy AllInOne project view and the SplitInfraCode switcher.
 */
@Component({
  selector: 'app-bootstrap-setup-guide',
  standalone: true,
  imports: [TranslateModule, MatIconModule],
  templateUrl: './bootstrap-setup-guide.component.html',
  styleUrl: './bootstrap-setup-guide.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BootstrapSetupGuideComponent {}

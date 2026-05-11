import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

import { environment } from '../../../../environments/environment';

// NOTE: `version` is read from environment with a 'dev' fallback. Build-time injection will
// later be wired through the Angular build system to expose the package.json version.
const VersionFallback = 'dev';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss',
})
export class FooterComponent {
  protected readonly version = environment.version ?? VersionFallback;
  protected readonly envName = environment.name ?? (environment.production ? 'production' : 'development');
  protected readonly docsUrl = 'https://aspire.dev';
}


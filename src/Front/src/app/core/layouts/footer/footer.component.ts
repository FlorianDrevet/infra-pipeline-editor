import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

import { environment } from '../../../../environments/environment';

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
  protected readonly currentYear = new Date().getFullYear();
  protected readonly docsUrl = 'https://docs.infraflowsculptor.dev';
  protected readonly termsUrl = 'https://infraflowsculptor.dev/terms';
  protected readonly privacyUrl = 'https://infraflowsculptor.dev/privacy';
  protected readonly statusUrl = 'https://status.infraflowsculptor.dev';
}


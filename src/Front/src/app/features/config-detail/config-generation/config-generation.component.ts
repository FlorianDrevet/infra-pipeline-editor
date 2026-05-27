import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { InfraConfigService } from '../../../shared/services/infra-config.service';

const CONFIG_ROUTE_SEGMENT = '/config';
const GENERATE_ROUTE_SEGMENT = 'generate';
const HOME_ROUTE = '/';
const PROJECTS_ROUTE_SEGMENT = '/projects';

/**
 * Legacy bridge route kept for backward compatibility.
 * Redirects to the canonical project generation page when the config project can be resolved.
 */
@Component({
  selector: 'app-config-generation',
  standalone: true,
  imports: [],
  template: '',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigGenerationComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly infraConfigService = inject(InfraConfigService);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.runNavigation(this.redirectToCanonicalGenerationPage(id));
    } else {
      this.runNavigation(this.router.navigate([HOME_ROUTE]));
    }
  }

  private runNavigation(navigationPromise: Promise<boolean>): void {
    navigationPromise.catch(() => undefined);
  }

  private async redirectToCanonicalGenerationPage(configId: string): Promise<boolean> {
    try {
      const config = await this.infraConfigService.getById(configId);
      if (config.projectId) {
        return await this.router.navigate([PROJECTS_ROUTE_SEGMENT, config.projectId, GENERATE_ROUTE_SEGMENT]);
      }
    } catch {
      // Safe fallback for legacy deep links when the config lookup fails.
    }

    return this.router.navigate([CONFIG_ROUTE_SEGMENT, configId]);
  }
}

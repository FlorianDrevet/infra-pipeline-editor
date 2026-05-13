import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

/**
 * Dedicated generation page for a configuration.
 * Currently redirects to config-detail with generation auto-triggered.
 * In a future iteration, this will be a full standalone generation view.
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

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.runNavigation(this.router.navigate(['/config', id], { queryParams: { generate: true } }));
    } else {
      this.runNavigation(this.router.navigate(['/']));
    }
  }

  private runNavigation(navigationPromise: Promise<boolean>): void {
    navigationPromise.catch(() => undefined);
  }
}

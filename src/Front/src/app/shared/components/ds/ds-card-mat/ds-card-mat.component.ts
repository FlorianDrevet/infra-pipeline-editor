import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { DsCardMatTone } from './ds-card-mat.types';

/**
 * Design system card primitive used to replace ad-hoc `mat-card` chains.
 *
 * Provides formal `title` and `subtitle` inputs plus three named content
 * projection slots: `[ds-card-header]` (extra header content beneath the
 * built-in title block), `[ds-card-content]` (body) and `[ds-card-actions]`
 * (right-aligned footer actions). The surface uses tokens `--ifs-surface-2`,
 * `--ifs-border-subtle` and `--ifs-radius-lg`.
 */
@Component({
  selector: 'app-ds-card-mat',
  standalone: true,
  imports: [],
  templateUrl: './ds-card-mat.component.html',
  styleUrl: './ds-card-mat.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsCardMatComponent {
  public readonly title = input<string | undefined>(undefined);
  public readonly subtitle = input<string | undefined>(undefined);
  public readonly tone = input<DsCardMatTone>('neutral');

  protected readonly classes = computed<string>(() => {
    return ['ds-card-mat', `ds-card-mat--tone-${this.tone()}`].join(' ');
  });

  protected readonly hasTitleBlock = computed<boolean>(() => {
    return Boolean(this.title()) || Boolean(this.subtitle());
  });
}

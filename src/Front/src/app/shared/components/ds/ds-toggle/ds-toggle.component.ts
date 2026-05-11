import {
  ChangeDetectionStrategy,
  Component,
  forwardRef,
  input,
} from '@angular/core';

import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { DsBooleanControlBase } from '../ds-boolean-control-base';
import { DsToggleLabelPosition } from './ds-toggle.types';

/**
 * Design system slide toggle. Pill 32×18, accent background when checked.
 */
@Component({
  selector: 'app-ds-toggle',
  standalone: true,
  imports: [],
  templateUrl: './ds-toggle.component.html',
  styleUrl: './ds-toggle.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsToggleComponent),
      multi: true,
    },
  ],
})
export class DsToggleComponent extends DsBooleanControlBase {
  public readonly ariaLabel = input<string | undefined>(undefined);
  public readonly labelPosition = input<DsToggleLabelPosition>('after');
}


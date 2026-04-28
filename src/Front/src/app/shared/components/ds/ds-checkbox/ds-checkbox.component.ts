import {
  ChangeDetectionStrategy,
  Component,
  forwardRef,
  input,
} from '@angular/core';

import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { DsBooleanControlBase } from '../ds-boolean-control-base';

/**
 * Design system checkbox. Square, brand blue when checked, supports indeterminate.
 */
@Component({
  selector: 'app-ds-checkbox',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './ds-checkbox.component.html',
  styleUrl: './ds-checkbox.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DsCheckboxComponent),
      multi: true,
    },
  ],
})
export class DsCheckboxComponent extends DsBooleanControlBase {
  public readonly indeterminate = input<boolean>(false);
}

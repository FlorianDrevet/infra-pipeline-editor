import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';

import {
  CompactSelectComponent,
  CompactSelectOption,
} from '../../../../shared/components/compact-select/compact-select.component';
import { ResourceEditKvMissingRoleEntry } from './resource-edit-kv-missing-role-entry.interface';

@Component({
  selector: 'app-resource-edit-kv-missing-role-card',
  standalone: true,
  imports: [
    CompactSelectComponent,
    MatButtonModule,
    MatButtonToggleModule,
    MatIconModule,
    MatProgressSpinnerModule,
    TranslateModule,
  ],
  templateUrl: './resource-edit-kv-missing-role-card.component.html',
  styleUrl: './resource-edit-kv-missing-role-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditKvMissingRoleCardComponent {
  readonly canWrite = input.required<boolean>();
  readonly entry = input.required<ResourceEditKvMissingRoleEntry>();
  readonly assignedUai = input<{ identityId: string; identityName: string } | null>(null);
  readonly uaiOptions = input<CompactSelectOption[]>([]);

  readonly identityTypeChange = output<ResourceEditKvMissingRoleEntry['selectedIdentityType']>();
  readonly uaiIdChange = output<string | null>();
  readonly createUai = output<void>();
  readonly assignRole = output<void>();

  protected onIdentityTypeChange(identityType: ResourceEditKvMissingRoleEntry['selectedIdentityType']): void {
    this.identityTypeChange.emit(identityType);
  }

  protected onUaiIdChange(uaiId: string | null): void {
    this.uaiIdChange.emit(uaiId);
  }

  protected onCreateUai(): void {
    this.createUai.emit();
  }

  protected onAssignRole(): void {
    this.assignRole.emit();
  }
}
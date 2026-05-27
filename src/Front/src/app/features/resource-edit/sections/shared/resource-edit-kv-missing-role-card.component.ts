import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { DsButtonComponent } from '../../../../shared/components/ds/ds-button/ds-button.component';
import { DsSegmentedControlComponent } from '../../../../shared/components/ds/ds-segmented-control/ds-segmented-control.component';
import { DsSegmentedOption } from '../../../../shared/components/ds/ds-segmented-control/ds-segmented-control.types';
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
    DsButtonComponent,
    DsSegmentedControlComponent,
    FormsModule,
    MatIconModule,
    TranslateModule,
  ],
  templateUrl: './resource-edit-kv-missing-role-card.component.html',
  styleUrl: './resource-edit-kv-missing-role-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditKvMissingRoleCardComponent {
  private readonly translate = inject(TranslateService);

  readonly canWrite = input.required<boolean>();
  readonly entry = input.required<ResourceEditKvMissingRoleEntry>();
  readonly assignedUai = input<{ identityId: string; identityName: string } | null>(null);
  readonly uaiOptions = input<CompactSelectOption[]>([]);

  readonly identityTypeChange = output<ResourceEditKvMissingRoleEntry['selectedIdentityType']>();
  readonly uaiIdChange = output<string | null>();
  readonly createUai = output<void>();
  readonly assignRole = output<void>();

  protected readonly identityTypeOptions: readonly DsSegmentedOption[] = [
    { value: 'UserAssigned', label: this.translate.instant('RESOURCE_EDIT.APP_SETTINGS.KV_ON_UAI') },
    { value: 'SystemAssigned', label: this.translate.instant('RESOURCE_EDIT.APP_SETTINGS.KV_ON_SAI') },
  ];

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
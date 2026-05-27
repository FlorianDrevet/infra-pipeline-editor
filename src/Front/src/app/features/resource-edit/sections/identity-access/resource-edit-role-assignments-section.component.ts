import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { DsButtonComponent } from '../../../../shared/components/ds/ds-button/ds-button.component';
import { DsIconButtonComponent } from '../../../../shared/components/ds/ds-icon-button/ds-icon-button.component';
import { DsMenuDirective } from '../../../../shared/components/ds/ds-menu/ds-menu.directive';
import { DsMenuItem } from '../../../../shared/components/ds/ds-menu/ds-menu.types';
import { DsSpinnerComponent } from '../../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { DsTooltipDirective } from '../../../../shared/components/ds/ds-tooltip/ds-tooltip.directive';
import { RoleAssignmentResponse } from '../../../../shared/interfaces/role-assignment.interface';
import { RESOURCE_TYPE_ICONS } from '../../../../shared/resource-metadata/resource-type.metadata';
import { ResourceEditIdentityAccessSection } from './resource-edit-identity-access-section.interface';

const SWITCH_TO_SYSTEM_ITEM_ID = '__switch_to_system__';
const SWITCH_TO_SYSTEM_LABEL_KEY = 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.SWITCH_TO_SYSTEM';

@Component({
  selector: 'app-resource-edit-role-assignments-section',
  standalone: true,
  imports: [
    MatIconModule,
    MatTooltipModule,
    DsSpinnerComponent,
    DsButtonComponent,
    DsIconButtonComponent,
    DsMenuDirective,
    DsTooltipDirective,
    RouterLink,
    TranslateModule,
  ],
  templateUrl: './resource-edit-role-assignments-section.component.html',
  styleUrl: './resource-edit-role-assignments-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditRoleAssignmentsSectionComponent {
  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  private readonly translate = inject(TranslateService);

  readonly canWrite = input.required<boolean>();
  readonly configId = input.required<string>();
  readonly section = input.required<ResourceEditIdentityAccessSection>();

  protected readonly switchUaiMenuItems = computed<DsMenuItem[]>(() =>
    this.section()
      .switchableIdentities()
      .map((identity) => ({
        id: identity.id,
        label: identity.name,
        icon: 'person',
      })),
  );

  protected readonly assignUaiMenuItems = computed<DsMenuItem[]>(() =>
    this.section()
      .availableUserAssignedIdentities()
      .map((identity) => ({
        id: identity.id,
        label: identity.name,
        icon: 'person',
      })),
  );

  protected buildChildIdentityMenuItems(assignment: RoleAssignmentResponse): DsMenuItem[] {
    const items: DsMenuItem[] = [
      {
        id: SWITCH_TO_SYSTEM_ITEM_ID,
        label: this.translate.instant(SWITCH_TO_SYSTEM_LABEL_KEY),
        icon: 'fingerprint',
        disabled: this.section().roleRequiresUserAssignedIdentity(assignment.roleDefinitionId),
      },
    ];

    for (const identity of this.section().availableUserAssignedIdentities()) {
      const isCurrent = identity.id === assignment.userAssignedIdentityId;
      items.push({
        id: identity.id,
        label: identity.name,
        icon: 'person',
        iconTrailing: isCurrent ? 'check' : undefined,
        disabled: isCurrent,
      });
    }

    return items;
  }

  protected buildSystemAssignMenuItems(): DsMenuItem[] {
    return this.section()
      .availableUserAssignedIdentities()
      .map((identity) => ({
        id: identity.id,
        label: identity.name,
        icon: 'person',
      }));
  }

  protected onAssignUaiSelected(item: DsMenuItem): void {
    void this.section().assignUaiToResource(item.id);
  }

  protected onChildIdentitySelected(item: DsMenuItem, assignment: RoleAssignmentResponse): void {
    if (item.id === SWITCH_TO_SYSTEM_ITEM_ID) {
      void this.section().switchToSystemAssigned(assignment);
      return;
    }
    void this.section().switchToUserAssignedIdentity(assignment, item.id);
  }

  protected onSystemAssignIdentitySelected(item: DsMenuItem, assignment: RoleAssignmentResponse): void {
    void this.section().switchToUserAssignedIdentity(assignment, item.id);
  }
}
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { AzureRoleDefinitionResponse } from '../../../shared/interfaces/role-assignment.interface';
import { RoleAssignmentService } from '../../../shared/services/role-assignment.service';
import { UserAssignedIdentityService } from '../../../shared/services/user-assigned-identity.service';
import {
  AddRoleAssignmentDialogComponent,
  AddRoleAssignmentDialogData,
} from './add-role-assignment-dialog.component';

interface TestSignal<T> {
  (): T;
  set(value: T): void;
}

interface AddRoleAssignmentDialogComponentTestApi {
  availableRoles: TestSignal<AzureRoleDefinitionResponse[]>;
  selectedIdentityType: TestSignal<string>;
  step: TestSignal<1 | 2 | 3>;
  onRoleChange(roleId: string): void;
}

function createRole(id: string, requiresUserAssignedIdentity: boolean): AzureRoleDefinitionResponse {
  return {
    id,
    name: `Role ${id}`,
    description: 'Role description',
    documentationUrl: 'https://example.test/role-doc',
    requiresUserAssignedIdentity,
  } as unknown as AzureRoleDefinitionResponse;
}

describe('AddRoleAssignmentDialogComponent', () => {
  let fixture: ComponentFixture<AddRoleAssignmentDialogComponent>;
  let componentTestApi: AddRoleAssignmentDialogComponentTestApi;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddRoleAssignmentDialogComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            sourceResourceId: 'source-1',
            currentResourceName: 'web-app',
            siblingResources: [],
            resourceGroupId: 'rg-1',
            configLocation: 'francecentral',
          } satisfies AddRoleAssignmentDialogData,
        },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<AddRoleAssignmentDialogComponent>>('MatDialogRef', ['close']),
        },
        {
          provide: RoleAssignmentService,
          useValue: jasmine.createSpyObj<RoleAssignmentService>('RoleAssignmentService', ['getAvailableRoleDefinitions', 'add']),
        },
        {
          provide: UserAssignedIdentityService,
          useValue: jasmine.createSpyObj<UserAssignedIdentityService>('UserAssignedIdentityService', ['create']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddRoleAssignmentDialogComponent);
    componentTestApi = fixture.componentInstance as unknown as AddRoleAssignmentDialogComponentTestApi;
    fixture.detectChanges();
  });

  it('switches to UserAssigned when the selected role requires a user-assigned identity', () => {
    const userAssignedOnlyRole = createRole('role-uai-only', true);
    componentTestApi.availableRoles.set([userAssignedOnlyRole]);
    componentTestApi.selectedIdentityType.set('SystemAssigned');

    componentTestApi.onRoleChange(userAssignedOnlyRole.id);

    expect(componentTestApi.selectedIdentityType()).toBe('UserAssigned');
  });

  it('shows the user-assigned-only hint when the selected role requires it', () => {
    const userAssignedOnlyRole = createRole('role-uai-only', true);
    componentTestApi.availableRoles.set([userAssignedOnlyRole]);
    componentTestApi.step.set(2);

    componentTestApi.onRoleChange(userAssignedOnlyRole.id);
    fixture.detectChanges();

    const hint = fixture.nativeElement.querySelector('.config-section__hint');

    expect(hint).not.toBeNull();
    expect(hint.textContent).toContain('RESOURCE_EDIT.ADD_ROLE_DIALOG.ACR_PULL_USER_ASSIGNED_ONLY');
  });
});
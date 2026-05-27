import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { ResourceEditUsedBySectionComponent } from './resource-edit-used-by-section.component';

describe('ResourceEditUsedBySectionComponent', () => {
  let fixture: ComponentFixture<ResourceEditUsedBySectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResourceEditUsedBySectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ResourceEditUsedBySectionComponent);
  });

  it('shows the empty state when the identity is not used by any other resource', () => {
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('section', createSection());
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.empty-state')).not.toBeNull();
  });

  it('delegates the unlink action to the section controller', () => {
    const group = {
      sourceResourceId: 'resource-1',
      sourceResourceName: 'api-web',
      sourceResourceType: 'WebApp',
      assignments: [],
    };
    const openUnlinkResourceFromIdentityDialog = jasmine.createSpy('openUnlinkResourceFromIdentityDialog');

    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('section', createSection({
      usedByResources: signal([group]),
      openUnlinkResourceFromIdentityDialog,
    }));
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('app-ds-button[icon="link_off"] button') as HTMLButtonElement).click();

    expect(openUnlinkResourceFromIdentityDialog).toHaveBeenCalledOnceWith(group);
  });
});

function createSection(overrides: Partial<Record<string, unknown>> = {}): Record<string, unknown> {
  return {
    allResources: signal([]),
    availableContainerRegistries: signal([]),
    availableLogAnalyticsWorkspaces: signal([]),
    availableUserAssignedIdentities: signal([]),
    uaiOptions: signal([]),
    assignedUai: signal(null),
    switchableIdentities: signal([]),
    roleAssignments: signal([]),
    roleAssignmentsLoading: signal(false),
    roleAssignmentsError: signal(''),
    groupedRoleAssignments: signal({ systemAssigned: [], uaiGroups: [] }),
    identityRoleAssignments: signal([]),
    identityRoleAssignmentsLoading: signal(false),
    identityRoleAssignmentsError: signal(''),
    usedByResources: signal([]),
    loadAllResources: async () => undefined,
    loadRoleAssignments: async () => undefined,
    loadIdentityRoleAssignments: async () => undefined,
    toggleUaiExpand: () => undefined,
    isUaiExpanded: () => false,
    resolveTargetName: () => '',
    resolveTargetType: () => '',
    resolveRoleName: () => '',
    resolveRoleDocUrl: () => '',
    roleRequiresUserAssignedIdentity: () => false,
    openAddRoleAssignmentDialog: () => undefined,
    assignUaiToResource: async () => undefined,
    openUnassignUaiDialog: () => undefined,
    switchToSystemAssigned: async () => undefined,
    switchToUserAssignedIdentity: async () => undefined,
    openRemoveRoleAssignmentDialog: async () => undefined,
    openUnlinkResourceFromIdentityDialog: () => undefined,
    ...overrides,
  };
}
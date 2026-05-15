import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { ResourceEditGrantedRightsSectionComponent } from './resource-edit-granted-rights-section.component';

describe('ResourceEditGrantedRightsSectionComponent', () => {
  let fixture: ComponentFixture<ResourceEditGrantedRightsSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResourceEditGrantedRightsSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ResourceEditGrantedRightsSectionComponent);
  });

  it('shows the empty state when there are no granted rights', () => {
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('section', createSection());
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.empty-state')).not.toBeNull();
  });

  it('delegates the add action to the section controller', () => {
    const openAddRoleAssignmentDialog = jasmine.createSpy('openAddRoleAssignmentDialog');

    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('section', createSection({ openAddRoleAssignmentDialog }));
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('.ra-add-btn') as HTMLButtonElement).click();

    expect(openAddRoleAssignmentDialog).toHaveBeenCalledOnceWith();
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
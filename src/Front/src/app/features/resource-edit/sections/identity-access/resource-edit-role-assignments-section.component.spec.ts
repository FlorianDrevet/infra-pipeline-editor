import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { ResourceEditRoleAssignmentsSectionComponent } from './resource-edit-role-assignments-section.component';

describe('ResourceEditRoleAssignmentsSectionComponent', () => {
  let fixture: ComponentFixture<ResourceEditRoleAssignmentsSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResourceEditRoleAssignmentsSectionComponent, TranslateModule.forRoot()],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(ResourceEditRoleAssignmentsSectionComponent);
  });

  it('shows the empty state when there are no role assignments and no assigned identity', () => {
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('configId', 'config-1');
    fixture.componentRef.setInput('section', createSection());
    fixture.detectChanges();

    expect(getElement('.empty-state')).not.toBeNull();
  });

  it('delegates the add action to the section controller', () => {
    const openAddRoleAssignmentDialog = jasmine.createSpy('openAddRoleAssignmentDialog');

    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('configId', 'config-1');
    fixture.componentRef.setInput('section', createSection({ openAddRoleAssignmentDialog }));
    fixture.detectChanges();

    getButton('.ra-add-btn').click();

    expect(openAddRoleAssignmentDialog).toHaveBeenCalledOnceWith();
  });

  function getButton(selector: string): HTMLButtonElement {
    return fixture.nativeElement.querySelector(selector) as HTMLButtonElement;
  }

  function getElement(selector: string): HTMLElement | null {
    return fixture.nativeElement.querySelector(selector) as HTMLElement | null;
  }
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
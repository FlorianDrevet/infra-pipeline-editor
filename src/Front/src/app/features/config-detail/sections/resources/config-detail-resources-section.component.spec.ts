import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { ConfigDetailResourcesSectionComponent } from './config-detail-resources-section.component';

describe('ConfigDetailResourcesSectionComponent', () => {
  let fixture: ComponentFixture<ConfigDetailResourcesSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfigDetailResourcesSectionComponent, TranslateModule.forRoot()],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfigDetailResourcesSectionComponent);
  });

  it('delegates add-child actions for expanded parent resources', () => {
    const parentResource = createResource('parent-resource-id', 'web-api', 'WebApp');
    const onOpenAddChildResourceDialog = jasmine.createSpy('onOpenAddChildResourceDialog');

    fixture.componentRef.setInput('viewModel', createViewModel({
      resourceGroups: [createResourceGroup('rg-1')],
      expandedRgId: 'rg-1',
      rgResources: { 'rg-1': [parentResource] },
      getGroupedResources: () => [{ resource: parentResource, isParent: true, children: [], incomingChildren: [] }],
      isParentExpanded: () => true,
      onOpenAddChildResourceDialog,
    }));
    fixture.detectChanges();

    getButton('.add-child-resource-btn').click();

    expect(onOpenAddChildResourceDialog).toHaveBeenCalledOnceWith(parentResource, 'rg-1');
  });

  it('delegates standalone resource deletion to the parent callback', () => {
    const resource = createResource('resource-id', 'function-api', 'FunctionApp');
    const onOpenDeleteResourceDialog = jasmine.createSpy('onOpenDeleteResourceDialog');

    fixture.componentRef.setInput('viewModel', createViewModel({
      resourceGroups: [createResourceGroup('rg-1')],
      expandedRgId: 'rg-1',
      rgResources: { 'rg-1': [resource] },
      getGroupedResources: () => [{ resource, isParent: false }],
      onOpenDeleteResourceDialog,
    }));
    fixture.detectChanges();

    getButton('.resource-item .resource-action-btn--delete').click();

    expect(onOpenDeleteResourceDialog).toHaveBeenCalledOnceWith(resource, 'rg-1');
  });

  function getButton(selector: string): HTMLButtonElement {
    return fixture.nativeElement.querySelector(selector) as HTMLButtonElement;
  }
});

function createViewModel(overrides: Partial<Record<string, unknown>> = {}): Record<string, unknown> {
  return {
    configId: 'config-1',
    canWrite: true,
    resourceGroups: [],
    sortedEnvironments: [],
    previewEnvOptions: [],
    previewEnvId: null,
    rgErrorKey: '',
    expandedRgId: null,
    rgResources: {},
    rgResourcesLoading: null,
    resourceTypeIcons: { WebApp: 'language', FunctionApp: 'functions' },
    storageAccountDetails: {},
    storageDetailsLoading: null,
    onSetPreviewEnvId: () => undefined,
    onOpenAddResourceGroupDialog: () => undefined,
    onToggleRgExpand: () => undefined,
    getGroupedResources: () => [],
    resolveNamingPreview: () => null,
    hasMissingEnvironments: () => false,
    getMissingEnvironments: () => [],
    hasResourceDiagnostics: () => false,
    getResourceDiagnostics: () => [],
    onOpenAddResourceDialog: () => undefined,
    onOpenDeleteResourceGroupDialog: () => undefined,
    onOpenDeleteResourceDialog: () => undefined,
    isParentExpanded: () => false,
    onToggleParentExpand: () => undefined,
    onToggleStorageParentExpand: () => undefined,
    getStorageSubResourceCount: () => 0,
    publicAccessI18nKey: () => 'CONFIG_DETAIL.RESOURCES.PUBLIC_ACCESS.NONE',
    onNavigateToStorageTab: () => undefined,
    onOpenAddStorageSubResourceDialog: () => undefined,
    onOpenAddChildResourceDialog: () => undefined,
    getUnparentedCrossConfigRefs: () => [],
    ...overrides,
  };
}

function createResourceGroup(id: string): Record<string, string> {
  return {
    id,
    name: `rg-${id}`,
    location: 'westeurope',
  };
}

function createResource(id: string, name: string, resourceType: string): Record<string, unknown> {
  return {
    id,
    name,
    resourceType,
    location: 'westeurope',
    isExisting: false,
    configuredEnvironments: [],
  };
}
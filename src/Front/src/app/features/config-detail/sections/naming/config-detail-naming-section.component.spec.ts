import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { InfrastructureConfigResponse } from '../../../../shared/interfaces/infra-config.interface';
import { ProjectResponse } from '../../../../shared/interfaces/project.interface';
import { ConfigDetailNamingSectionComponent } from './config-detail-naming-section.component';
import { ConfigDetailNamingSectionViewModel } from './config-detail-naming-section.view-model';

describe('ConfigDetailNamingSectionComponent', () => {
  let fixture: ComponentFixture<ConfigDetailNamingSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfigDetailNamingSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfigDetailNamingSectionComponent);
  });

  it('renders the design-system inheritance toggle instead of a material slide toggle', () => {
    fixture.componentRef.setInput('viewModel', createViewModel());
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;

    expect(host.querySelectorAll('app-ds-toggle').length).toBe(1);
    expect(host.querySelector('mat-slide-toggle')).toBeNull();
  });
});

function createViewModel(
  overrides: Partial<ConfigDetailNamingSectionViewModel> = {},
): ConfigDetailNamingSectionViewModel {
  return {
    config: createConfig(),
    project: createProject(),
    canWrite: true,
    useProjectNamingConventions: false,
    inheritanceLoading: false,
    namingActionKey: null,
    namingErrorKey: '',
    canAddResourceNamingTemplate: true,
    resourceTypeIcons: { StorageAccount: 'storage' },
    abbreviationDisplayItems: [],
    isNamingActionActive: () => false,
    isResourceNamingTemplateBusy: () => false,
    isAbbreviationBusy: () => false,
    onToggleInheritanceNaming: () => undefined,
    onOpenDefaultNamingTemplateDialog: () => undefined,
    onOpenResourceNamingTemplateDialog: () => undefined,
    onOpenRemoveResourceNamingTemplateDialog: () => undefined,
    onOpenEditAbbreviationDialog: () => undefined,
    onOpenResetAbbreviationDialog: () => undefined,
    ...overrides,
  };
}

function createConfig(
  overrides: Partial<InfrastructureConfigResponse> = {},
): InfrastructureConfigResponse {
  return {
    id: 'config-1',
    name: 'Config 1',
    defaultNamingTemplate: null,
    projectId: 'project-1',
    useProjectNamingConventions: false,
    resourceNamingTemplates: [],
    resourceAbbreviationOverrides: [],
    resourceGroupCount: 0,
    resourceCount: 0,
    crossConfigReferenceCount: 0,
    appPipelineMode: 'Standard',
    tags: [],
    ...overrides,
  };
}

function createProject(overrides: Partial<ProjectResponse> = {}): ProjectResponse {
  return {
    id: 'project-1',
    name: 'Project 1',
    members: [],
    environmentDefinitions: [],
    defaultNamingTemplate: null,
    resourceNamingTemplates: [],
    resourceAbbreviations: [],
    tags: [],
    agentPoolName: null,
    ...overrides,
  };
}
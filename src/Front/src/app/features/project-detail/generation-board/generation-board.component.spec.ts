import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { By } from '@angular/platform-browser';
import { convertToParamMap, provideRouter, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { BicepFileNode } from '../../../shared/components/bicep-file-panel/bicep-file-panel.component';
import {
  GenerateProjectBicepResponse,
  GenerateProjectBootstrapPipelineResponse,
  GenerateProjectPipelineResponse,
  ProjectResponse,
} from '../../../shared/interfaces/project.interface';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { ProjectLayoutPreset, ProjectRepositoryResponse } from '../../../shared/interfaces/project-repository.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { ProjectDetailGenerationWorkflowService } from '../project-detail-generation-workflow.service';
import { GenerationBoardComponent } from './generation-board.component';

class ProjectDetailGenerationWorkflowServiceStub {
  readonly validatingDiagnostics = signal(false);
  readonly projectGenerateAllLoading = signal(false);
  readonly projectBicepLoading = signal(false);
  readonly projectBicepResult = signal<GenerateProjectBicepResponse | null>(null);
  readonly projectBicepDownloading = signal(false);
  readonly projectInfraArtifactsDownloading = signal(false);
  readonly projectBicepErrorKey = signal('');
  readonly projectGenerationPanelCollapsed = signal(false);
  readonly projectPipelineLoading = signal(false);
  readonly projectPipelineResult = signal<GenerateProjectPipelineResponse | null>(null);
  readonly projectPipelineDownloading = signal(false);
  readonly projectCodeArtifactsDownloading = signal(false);
  readonly projectPipelineErrorKey = signal('');
  readonly projectBootstrapLoading = signal(false);
  readonly projectBootstrapResult = signal<GenerateProjectBootstrapPipelineResponse | null>(null);
  readonly projectBootstrapDownloading = signal(false);
  readonly projectBootstrapErrorKey = signal('');
  readonly canPushAllProjectArtifacts = signal(false);
  readonly isSplitInfraCodeLayout = signal(false);
  readonly projectBicepNodes = signal<BicepFileNode[]>([]);
  readonly projectPipelineNodes = signal<BicepFileNode[]>([]);
  readonly projectBootstrapNodes = signal<BicepFileNode[]>([]);
  readonly deferMonoRepoBatchReveal = signal(false);
  readonly projectGenerationPanelOpen = signal(false);

  readonly loadProjectBicepFile = jasmine.createSpy('loadProjectBicepFile').and.resolveTo('main content');
  readonly loadProjectPipelineFile = jasmine.createSpy('loadProjectPipelineFile').and.resolveTo('pipeline content');
  readonly loadProjectBootstrapFile = jasmine.createSpy('loadProjectBootstrapFile').and.resolveTo('bootstrap content');

  readonly setProject = jasmine.createSpy('setProject');
  readonly setConfigs = jasmine.createSpy('setConfigs');
  readonly generateAll = jasmine.createSpy('generateAll').and.resolveTo();
  readonly generateProjectBicep = jasmine.createSpy('generateProjectBicep').and.resolveTo();
  readonly generateProjectPipeline = jasmine.createSpy('generateProjectPipeline').and.resolveTo();
  readonly generateProjectBootstrap = jasmine.createSpy('generateProjectBootstrap').and.resolveTo();
  readonly toggleProjectGenerationPanelCollapsed = jasmine.createSpy('toggleProjectGenerationPanelCollapsed');
  readonly downloadProjectBicepFiles = jasmine.createSpy('downloadProjectBicepFiles').and.resolveTo();
  readonly downloadProjectInfraArtifacts = jasmine.createSpy('downloadProjectInfraArtifacts').and.resolveTo();
  readonly downloadProjectCodeArtifacts = jasmine.createSpy('downloadProjectCodeArtifacts').and.resolveTo();
  readonly openProjectPushAllToGitDialog = jasmine.createSpy('openProjectPushAllToGitDialog');
  readonly openProjectMultiRepoPushDialog = jasmine.createSpy('openProjectMultiRepoPushDialog');
  readonly downloadProjectPipelineFiles = jasmine.createSpy('downloadProjectPipelineFiles').and.resolveTo();
  readonly downloadProjectBootstrapFiles = jasmine.createSpy('downloadProjectBootstrapFiles').and.resolveTo();
}

describe('GenerationBoardComponent', () => {
  let fixture: ComponentFixture<GenerationBoardComponent>;
  let component: GenerationBoardComponent;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;
  let dialogSpy: jasmine.SpyObj<MatDialog>;
  let workflowStub: ProjectDetailGenerationWorkflowServiceStub;

  beforeEach(async () => {
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', ['getProject', 'getProjectConfigs']);
    dialogSpy = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);
    workflowStub = new ProjectDetailGenerationWorkflowServiceStub();

    projectServiceSpy.getProject.and.resolveTo(createProject());
    projectServiceSpy.getProjectConfigs.and.resolveTo([createConfig()]);

    TestBed.overrideComponent(GenerationBoardComponent, {
      remove: {
        providers: [ProjectDetailGenerationWorkflowService],
      },
      add: {
        providers: [{ provide: ProjectDetailGenerationWorkflowService, useValue: workflowStub }],
      },
    });

    await TestBed.configureTestingModule({
      imports: [GenerationBoardComponent, NoopAnimationsModule, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ id: 'project-1' }),
            },
          },
        },
        { provide: ProjectService, useValue: projectServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']) },
      ],
    }).compileComponents();
  });

  it('syncs the loaded project and configs into the generation workflow service', async () => {
    await createComponent();

    expect(workflowStub.setProject).toHaveBeenCalledWith(createProject());
    expect(workflowStub.setConfigs).toHaveBeenCalledWith([createConfig()]);
  });

  it('delegates the generate action to the workflow service instead of opening dialogs directly', async () => {
    await createComponent();

    invokeGenerateAll(component);

    expect(workflowStub.generateAll).toHaveBeenCalledOnceWith();
    expect(dialogSpy.open).not.toHaveBeenCalled();
  });

  it('removes the per-config list from repository cards', async () => {
    await createComponent();

    expect(fixture.nativeElement.querySelector('.repo-card__configs')).toBeNull();
  });

  it('renders the mono-repo explorer when generated nodes are available', async () => {
    await createComponent();
    workflowStub.projectGenerationPanelOpen.set(true);
    workflowStub.projectBicepResult.set(createBicepResponse());
    workflowStub.projectBicepNodes.set([createNode('main.bicep')]);

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('app-bicep-file-panel')).not.toBeNull();
  });

  it('renders the split infra/code explorer when the workflow is in split mode', async () => {
    projectServiceSpy.getProject.and.resolveTo(createProject('SplitInfraCode'));

    await createComponent();
    workflowStub.projectGenerationPanelOpen.set(true);
    workflowStub.isSplitInfraCodeLayout.set(true);

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('app-split-generation-switcher')).not.toBeNull();
  });

  async function createComponent(): Promise<void> {
    fixture = TestBed.createComponent(GenerationBoardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  }
});

function createProject(layoutPreset: ProjectLayoutPreset = 'AllInOne'): ProjectResponse {
  return {
    id: 'project-1',
    name: 'Project 1',
    description: 'Project description',
    members: [],
    environmentDefinitions: [],
    defaultNamingTemplate: null,
    resourceNamingTemplates: [],
    resourceAbbreviations: [],
    tags: [],
    agentPoolName: null,
    repositories: [createRepository(layoutPreset)],
    layoutPreset,
    usedResourceTypes: [],
  };
}

function createRepository(layoutPreset: ProjectLayoutPreset): ProjectRepositoryResponse {
  return {
    id: 'repo-1',
    alias: layoutPreset === 'SplitInfraCode' ? 'infra' : 'default',
    providerType: 'GitHub',
    repositoryUrl: 'https://example.test/org/repo',
    owner: 'org',
    repositoryName: 'repo',
    defaultBranch: 'main',
    contentKinds: layoutPreset === 'SplitInfraCode' ? ['Infrastructure'] : ['Infrastructure'],
  };
}

function createConfig(): InfrastructureConfigResponse {
  return {
    id: 'config-1',
    name: 'Config 1',
    defaultNamingTemplate: null,
    projectId: 'project-1',
    useProjectNamingConventions: true,
    resourceNamingTemplates: [],
    resourceAbbreviationOverrides: [],
    resourceGroupCount: 2,
    resourceCount: 5,
    crossConfigReferenceCount: 0,
    appPipelineMode: 'Combined',
    tags: [],
    layoutMode: null,
    repositories: [],
  };
}

function createBicepResponse(): GenerateProjectBicepResponse {
  return {
    commonFileUris: { 'main.bicep': '/files/main.bicep' },
    configFileUris: {},
  };
}

function createNode(path: string): BicepFileNode {
  return {
    kind: 'file',
    path,
    displayName: path,
    type: 'entry-point',
    uri: path,
    depth: 0,
    parentFolderKey: '',
  };
}

function invokeGenerateAll(component: GenerationBoardComponent): void {
  (component as unknown as { onGenerateAll(): void }).onGenerateAll();
}
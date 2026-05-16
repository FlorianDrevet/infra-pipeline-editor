import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { convertToParamMap, provideRouter, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { SidebarContextService } from '../../../core/layouts/sidebar/sidebar-context.service';
import {
  GenerateProjectBicepResponse,
  GenerateProjectBootstrapPipelineResponse,
  GenerateProjectPipelineResponse,
  ProjectResponse,
} from '../../../shared/interfaces/project.interface';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { ProjectLayoutPreset } from '../../../shared/interfaces/project-repository.interface';
import { LanguageService } from '../../../shared/services/language.service';
import { ProjectService } from '../../../shared/services/project.service';
import { ProjectDetailGenerationWorkflowService } from '../project-detail-generation-workflow.service';
import { GenerationBoardComponent } from './generation-board.component';

class ProjectDetailGenerationWorkflowServiceBannerStub {
  readonly validatingDiagnostics = signal(false);
  readonly lastGenerationLoading = signal(false);
  readonly lastGenerationAvailable = signal<boolean | null>(true);
  readonly lastGenerationErrorKey = signal('');
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
  readonly projectBicepNodes = signal([]);
  readonly projectPipelineNodes = signal([]);
  readonly projectBootstrapNodes = signal([]);
  readonly deferMonoRepoBatchReveal = signal(false);
  readonly projectGenerationPanelOpen = signal(true);
  readonly viewingHistoricalGeneration = signal(true);
  readonly displayedHistoricalGenerationAt = signal('2026-05-16T10:30:00Z');

  readonly loadProjectBicepFile = jasmine.createSpy('loadProjectBicepFile').and.resolveTo('main content');
  readonly loadProjectPipelineFile = jasmine.createSpy('loadProjectPipelineFile').and.resolveTo('pipeline content');
  readonly loadProjectBootstrapFile = jasmine.createSpy('loadProjectBootstrapFile').and.resolveTo('bootstrap content');

  readonly setProject = jasmine.createSpy('setProject');
  readonly setConfigs = jasmine.createSpy('setConfigs');
  readonly checkLastGenerationAvailable = jasmine.createSpy('checkLastGenerationAvailable').and.resolveTo();
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
  readonly loadLastGeneration = jasmine.createSpy('loadLastGeneration').and.resolveTo();
}

describe('GenerationBoardComponent historical generation banner', () => {
  let fixture: ComponentFixture<GenerationBoardComponent>;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;
  let sidebarContextServiceSpy: jasmine.SpyObj<SidebarContextService>;
  let workflowStub: ProjectDetailGenerationWorkflowServiceBannerStub;

  beforeEach(async () => {
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', ['getProject', 'getProjectConfigs']);
    sidebarContextServiceSpy = jasmine.createSpyObj<SidebarContextService>('SidebarContextService', ['setProjectContext']);
    workflowStub = new ProjectDetailGenerationWorkflowServiceBannerStub();

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
      imports: [GenerationBoardComponent, TranslateModule.forRoot()],
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
        { provide: MatDialog, useValue: jasmine.createSpyObj<MatDialog>('MatDialog', ['open']) },
        { provide: MatSnackBar, useValue: jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']) },
        { provide: SidebarContextService, useValue: sidebarContextServiceSpy },
        {
          provide: LanguageService,
          useValue: {
            currentLanguage: signal<'fr' | 'en'>('en'),
          } satisfies Pick<LanguageService, 'currentLanguage'>,
        },
      ],
    }).compileComponents();

    const translateService = TestBed.inject(TranslateService);
    translateService.setTranslation('en', {
      PROJECT_DETAIL: {
        BOARD: {
          PAGE_TITLE: 'Generation',
          PAGE_SUBTITLE: 'Generation for {{projectName}}',
          PROJECT_PLACEHOLDER: 'Project',
          BREADCRUMB_CURRENT: 'Generation',
          SUMMARY_LAYOUT: 'Layout',
          SUMMARY_REPOSITORIES: 'Repositories',
          SUMMARY_CONFIGS: 'Configurations',
          SUMMARY_RESOURCE_GROUPS: 'Resource groups',
          SUMMARY_RESOURCES: 'Resources',
          HISTORICAL_GENERATION_TITLE: 'Viewing a previous generation',
          HISTORICAL_GENERATION_DESCRIPTION: 'Artifacts loaded from the generation created on {{generatedAt}}.',
          HISTORICAL_GENERATION_ACTION: 'Generate a new version',
        },
        LAYOUT: {
          PRESET_ALL_IN_ONE: 'All-in-one',
        },
      },
      PROJECTS: {
        TITLE: 'Projects',
      },
    }, true);
    translateService.use('en');
  });

  it('renders a historical generation banner with a relaunch action', async () => {
    fixture = TestBed.createComponent(GenerationBoardComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    await fixture.whenRenderingDone();
    fixture.detectChanges();

    const nativeElement = fixture.nativeElement as HTMLElement;
    const banner = nativeElement.querySelector('app-ds-banner');
    const actionButton = Array.from(nativeElement.querySelectorAll('app-ds-banner app-ds-button button'))
      .find((button) => button.textContent?.includes('Generate a new version'));

    expect(banner).not.toBeNull();
    expect(nativeElement.textContent).toContain('Viewing a previous generation');
    expect(nativeElement.textContent).toContain('May 16, 2026');
    expect(actionButton).toBeTruthy();
  });

  it('delegates the banner relaunch action to generateAll', async () => {
    fixture = TestBed.createComponent(GenerationBoardComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    await fixture.whenRenderingDone();
    fixture.detectChanges();

    const actionButton = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('app-ds-banner app-ds-button button'))
      .find((button) => button.textContent?.includes('Generate a new version'));

    if (!actionButton) {
      fail('Expected the historical generation banner action button to be rendered.');
      return;
    }

    actionButton.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    fixture.detectChanges();

    expect(workflowStub.generateAll).toHaveBeenCalledOnceWith();
  });
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
    repositories: [
      {
        id: 'repo-1',
        alias: 'default',
        providerType: 'GitHub',
        repositoryUrl: 'https://example.test/org/default',
        owner: 'org',
        repositoryName: 'default',
        defaultBranch: 'main',
        contentKinds: ['Infrastructure'],
      },
    ],
    layoutPreset,
    usedResourceTypes: [],
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
import { TestBed } from '@angular/core/testing';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { of } from 'rxjs';

import {
  GenerateProjectBicepResponse,
  GenerateProjectBootstrapPipelineResponse,
  GenerateProjectPipelineResponse,
  GetProjectLatestGenerationResponse,
  ProjectResponse,
} from '../../shared/interfaces/project.interface';
import { ResourceDiagnosticResponse } from '../../shared/interfaces/config-diagnostics.interface';
import { InfrastructureConfigResponse } from '../../shared/interfaces/infra-config.interface';
import { CustomDomainResponse } from '../../shared/interfaces/custom-domain.interface';
import { ProjectService } from '../../shared/services/project.service';
import { CustomDomainService } from '../../shared/services/custom-domain.service';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { ResourceGroupService } from '../../shared/services/resource-group.service';
import { AzureResourceResponse, ResourceGroupResponse } from '../../shared/interfaces/resource-group.interface';
import { ProjectDetailGenerationWorkflowService } from './project-detail-generation-workflow.service';

describe('ProjectDetailGenerationWorkflowService', () => {
  let service: ProjectDetailGenerationWorkflowService;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;
  let customDomainServiceSpy: jasmine.SpyObj<CustomDomainService>;
  let infraConfigServiceSpy: jasmine.SpyObj<InfraConfigService>;
  let resourceGroupServiceSpy: jasmine.SpyObj<ResourceGroupService>;
  let dialogSpy: jasmine.SpyObj<MatDialog>;

  beforeEach(() => {
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', [
      'generateProjectBicep',
      'generateProjectPipeline',
      'generateProjectBootstrapPipeline',
      'getProjectLatestGeneration',
    ]);
    customDomainServiceSpy = jasmine.createSpyObj<CustomDomainService>('CustomDomainService', ['getByResourceId']);
    infraConfigServiceSpy = jasmine.createSpyObj<InfraConfigService>('InfraConfigService', [
      'getDiagnostics',
      'getResourceGroups',
    ]);
    resourceGroupServiceSpy = jasmine.createSpyObj<ResourceGroupService>('ResourceGroupService', ['getResources']);
    dialogSpy = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

    projectServiceSpy.generateProjectBicep.and.resolveTo(createBicepResponse());
    projectServiceSpy.generateProjectPipeline.and.resolveTo(createPipelineResponse());
    projectServiceSpy.generateProjectBootstrapPipeline.and.resolveTo(createBootstrapResponse());
    projectServiceSpy.getProjectLatestGeneration.and.resolveTo(null);

    TestBed.configureTestingModule({
      providers: [
        ProjectDetailGenerationWorkflowService,
        { provide: ProjectService, useValue: projectServiceSpy },
        { provide: CustomDomainService, useValue: customDomainServiceSpy },
        { provide: InfraConfigService, useValue: infraConfigServiceSpy },
        { provide: ResourceGroupService, useValue: resourceGroupServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']) },
        { provide: TranslateService, useValue: { instant: (key: string) => key } },
      ],
    });

    service = TestBed.inject(ProjectDetailGenerationWorkflowService);
    service.setProject(createProject());
    service.setConfigs([]);
  });

  it('runs bicep, pipeline, and bootstrap generation together for generateAll', async () => {
    await service.generateAll();

    expect(projectServiceSpy.generateProjectBicep).toHaveBeenCalledOnceWith('project-1');
    expect(projectServiceSpy.generateProjectPipeline).toHaveBeenCalledOnceWith('project-1');
    expect(projectServiceSpy.generateProjectBootstrapPipeline).toHaveBeenCalledOnceWith('project-1');
    expect(service.projectBicepResult()).toEqual(createBicepResponse());
    expect(service.projectPipelineResult()).toEqual(createPipelineResponse());
    expect(service.projectBootstrapResult()).toEqual(createBootstrapResponse());
  });

  it('opens the configuration-owned bulk push dialog for MultiRepo projects', () => {
    service.setProject({ ...createProject(), layoutPreset: 'MultiRepo' });
    service.setConfigs([{ ...createConfig(), layoutMode: 'AllInOne', repositories: [createConfigRepository()] }]);

    service.openProjectMultiRepoArtifactsPushDialog();

    expect(dialogSpy.open).toHaveBeenCalled();
    const dialogConfig = dialogSpy.open.calls.mostRecent().args[1] as {
      data: {
        projectId: string;
        configurations: InfrastructureConfigResponse[];
      };
    };
    expect(dialogConfig.data.projectId).toBe('project-1');
    expect(dialogConfig.data.configurations[0].repositories?.[0].id).toBe('config-repository-1');
  });

  it('stops generation when diagnostics dialog is rejected', async () => {
    service.setConfigs([createConfig()]);
    infraConfigServiceSpy.getDiagnostics.and.resolveTo({ diagnostics: [createDiagnostic()] });
    infraConfigServiceSpy.getResourceGroups.and.resolveTo([]);
    dialogSpy.open.and.returnValue(createClosedDialogRef(false));

    await service.generateProjectBicep();

    expect(dialogSpy.open).toHaveBeenCalled();
    expect(projectServiceSpy.generateProjectBicep).not.toHaveBeenCalled();
    expect(service.projectBicepResult()).toBeNull();
  });

  it('shows the diagnostics dialog when pending custom domains exist', async () => {
    service.setConfigs([createConfig()]);
    infraConfigServiceSpy.getDiagnostics.and.resolveTo({ diagnostics: [] });
    infraConfigServiceSpy.getResourceGroups.and.resolveTo([createResourceGroup()]);
    resourceGroupServiceSpy.getResources.and.resolveTo([createContainerAppResource()]);
    customDomainServiceSpy.getByResourceId.and.resolveTo([createPendingCustomDomain()]);
    dialogSpy.open.and.returnValue(createClosedDialogRef(false));

    await service.generateProjectBicep();

    expect(customDomainServiceSpy.getByResourceId).toHaveBeenCalledOnceWith('resource-1');
    expect(dialogSpy.open).toHaveBeenCalled();
    expect(projectServiceSpy.generateProjectBicep).not.toHaveBeenCalled();

    const dialogConfig = dialogSpy.open.calls.mostRecent().args[1] as {
      data: {
        pendingCustomDomainConfigs?: Array<{
          configId: string;
          configName: string;
          domains: Array<{
            resourceId: string;
            resourceName: string;
            resourceType: string;
            domainName: string;
            environmentName: string;
          }>;
        }>;
      };
    };

    expect(dialogConfig.data.pendingCustomDomainConfigs).toEqual([
      {
        configId: 'config-1',
        configName: 'Config 1',
        domains: [
          {
            resourceId: 'resource-1',
            resourceName: 'ifs-frontend',
            resourceType: 'ContainerApp',
            domainName: 'infraflowsculptor.fr',
            environmentName: 'dev',
          },
        ],
      },
    ]);
  });

  it('marks the last generation as expired when the endpoint returns null', async () => {
    projectServiceSpy.getProjectLatestGeneration.and.resolveTo(null);

    await service.loadLastGeneration();

    expect(service.lastGenerationErrorKey()).toBe('PROJECT_DETAIL.BOARD.LAST_GENERATION_EXPIRED');
    expect(service.lastGenerationLoading()).toBeFalse();
  });

  it('reuses the latest generation fetched for availability when historical artifacts are loaded', async () => {
    const latestGeneration = createLatestGenerationResponse();
    projectServiceSpy.getProjectLatestGeneration.and.resolveTo(latestGeneration);

    await service.checkLastGenerationAvailable();
    await service.loadLastGeneration();

    expect(projectServiceSpy.getProjectLatestGeneration).toHaveBeenCalledTimes(1);
    expect(service.lastGenerationAvailable()).toBeTrue();
    expect(service.projectBicepResult()).toEqual({
      commonFileUris: latestGeneration.bicep!.commonFileUris,
      configFileUris: latestGeneration.bicep!.configFileUris,
    });
  });

  it('surfaces a last generation error when the endpoint fails', async () => {
    projectServiceSpy.getProjectLatestGeneration.and.rejectWith(new Error('latest generation failed'));

    await service.loadLastGeneration();

    expect(service.lastGenerationErrorKey()).toBe('PROJECT_DETAIL.BOARD.LAST_GENERATION_ERROR');
    expect(service.lastGenerationLoading()).toBeFalse();
  });

  it('stores the viewed historical generation timestamp when the latest generation is loaded', async () => {
    const latestGeneration = createLatestGenerationResponse();
    projectServiceSpy.getProjectLatestGeneration.and.resolveTo(latestGeneration);

    await service.loadLastGeneration();

    expect(service.viewingHistoricalGeneration()).toBeTrue();
    expect(service.displayedHistoricalGenerationAt()).toBe(latestGeneration.generatedAt);
    expect(service.projectBicepResult()).toEqual({
      commonFileUris: latestGeneration.bicep!.commonFileUris,
      configFileUris: latestGeneration.bicep!.configFileUris,
    });
  });

  it('clears the historical generation context when a new project generation starts', async () => {
    projectServiceSpy.getProjectLatestGeneration.and.resolveTo(createLatestGenerationResponse());

    await service.loadLastGeneration();
    await service.generateProjectBicep();

    expect(service.viewingHistoricalGeneration()).toBeFalse();
    expect(service.displayedHistoricalGenerationAt()).toBeNull();
    expect(projectServiceSpy.generateProjectBicep).toHaveBeenCalledOnceWith('project-1');
  });
});

function createProject(): ProjectResponse {
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
    repositories: [],
    layoutPreset: 'AllInOne',
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
    resourceGroupCount: 0,
    resourceCount: 0,
    crossConfigReferenceCount: 0,
    appPipelineMode: 'Combined',
    tags: [],
    layoutMode: null,
    repositories: [],
  };
}

function createDiagnostic(): ResourceDiagnosticResponse {
  return {
    resourceId: 'resource-1',
    resourceName: 'demo',
    resourceType: 'ContainerApp',
    severity: 'Error',
    ruleCode: 'RULE001',
    targetResourceName: 'demo',
  };
}

function createBicepResponse(): GenerateProjectBicepResponse {
  return {
    commonFileUris: { 'main.bicep': '/files/main.bicep' },
    configFileUris: {},
  };
}

function createPipelineResponse(): GenerateProjectPipelineResponse {
  return {
    commonFileUris: { 'azure-pipelines.yml': '/files/azure-pipelines.yml' },
    configFileUris: {},
    infraCommonFileUris: {},
    appCommonFileUris: {},
    infraConfigFileUris: {},
    appConfigFileUris: {},
  };
}

function createBootstrapResponse(): GenerateProjectBootstrapPipelineResponse {
  return {
    fileUris: { 'bootstrap.yml': '/files/bootstrap.yml' },
    infraFileUris: {},
    appFileUris: {},
  };
}

function createLatestGenerationResponse(): GetProjectLatestGenerationResponse {
  return {
    bicep: {
      commonFileUris: { 'main.bicep': '/files/main.bicep' },
      configFileUris: {},
    },
    pipeline: null,
    bootstrap: null,
    generatedAt: '2026-05-16T10:30:00Z',
  };
}

function createResourceGroup(): ResourceGroupResponse {
  return {
    id: 'rg-1',
    infraConfigId: 'config-1',
    name: 'rg-demo',
    location: 'westeurope',
  };
}

function createContainerAppResource(): AzureResourceResponse {
  return {
    id: 'resource-1',
    resourceType: 'ContainerApp',
    name: 'ifs-frontend',
    location: 'westeurope',
    configuredEnvironments: ['dev'],
  };
}

function createPendingCustomDomain(): CustomDomainResponse {
  return {
    id: 'domain-1',
    resourceId: 'resource-1',
    environmentName: 'dev',
    domainName: 'infraflowsculptor.fr',
    bindingType: 'SniEnabled',
    dnsValidationStatus: 'Pending',
  };
}

function createClosedDialogRef(result: boolean): MatDialogRef<unknown, boolean> {
  return {
    afterClosed: () => of(result),
  } as unknown as MatDialogRef<unknown, boolean>;
}

function createConfigRepository(): NonNullable<InfrastructureConfigResponse['repositories']>[number] {
  return {
    id: 'config-repository-1',
    providerType: 'GitHub',
    repositoryUrl: 'https://github.com/example/infra',
    owner: 'example',
    repositoryName: 'infra',
    defaultBranch: 'main',
    contentKinds: ['Infrastructure', 'ApplicationCode'],
  };
}
import { TestBed } from '@angular/core/testing';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { of } from 'rxjs';

import {
  GenerateProjectBicepResponse,
  GenerateProjectBootstrapPipelineResponse,
  GenerateProjectPipelineResponse,
  ProjectResponse,
} from '../../shared/interfaces/project.interface';
import { ResourceDiagnosticResponse } from '../../shared/interfaces/config-diagnostics.interface';
import { InfrastructureConfigResponse } from '../../shared/interfaces/infra-config.interface';
import { ProjectService } from '../../shared/services/project.service';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { ResourceGroupService } from '../../shared/services/resource-group.service';
import { ProjectDetailGenerationWorkflowService } from './project-detail-generation-workflow.service';

describe('ProjectDetailGenerationWorkflowService', () => {
  let service: ProjectDetailGenerationWorkflowService;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;
  let infraConfigServiceSpy: jasmine.SpyObj<InfraConfigService>;
  let resourceGroupServiceSpy: jasmine.SpyObj<ResourceGroupService>;
  let dialogSpy: jasmine.SpyObj<MatDialog>;

  beforeEach(() => {
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', [
      'generateProjectBicep',
      'generateProjectPipeline',
      'generateProjectBootstrapPipeline',
    ]);
    infraConfigServiceSpy = jasmine.createSpyObj<InfraConfigService>('InfraConfigService', [
      'getDiagnostics',
      'getResourceGroups',
    ]);
    resourceGroupServiceSpy = jasmine.createSpyObj<ResourceGroupService>('ResourceGroupService', ['getResources']);
    dialogSpy = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

    projectServiceSpy.generateProjectBicep.and.resolveTo(createBicepResponse());
    projectServiceSpy.generateProjectPipeline.and.resolveTo(createPipelineResponse());
    projectServiceSpy.generateProjectBootstrapPipeline.and.resolveTo(createBootstrapResponse());

    TestBed.configureTestingModule({
      providers: [
        ProjectDetailGenerationWorkflowService,
        { provide: ProjectService, useValue: projectServiceSpy },
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

  it('stops generation when diagnostics dialog is rejected', async () => {
    service.setConfigs([createConfig()]);
    infraConfigServiceSpy.getDiagnostics.and.resolveTo({ diagnostics: [createDiagnostic()] });
    infraConfigServiceSpy.getResourceGroups.and.resolveTo([]);
    dialogSpy.open.and.returnValue({ afterClosed: () => of(false) } as MatDialogRef<unknown, boolean>);

    await service.generateProjectBicep();

    expect(dialogSpy.open).toHaveBeenCalled();
    expect(projectServiceSpy.generateProjectBicep).not.toHaveBeenCalled();
    expect(service.projectBicepResult()).toBeNull();
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
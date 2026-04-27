import { inject, Injectable } from '@angular/core';
import axios from 'axios';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  ProjectResponse,
  CreateProjectRequest,
  CreateProjectWithSetupRequest,
  AddProjectMemberRequest,
  UpdateProjectMemberRoleRequest,
  RecentItemResponse,
  ValidateRecentItemsRequest,
  TestGitConnectionResponse,
  GitBranchResponse,
  GitFileResponse,
  AddProjectEnvironmentRequest,
  UpdateProjectEnvironmentRequest,
  GenerateProjectBicepResponse,
  GenerateProjectPipelineResponse,
  GenerateProjectBootstrapPipelineResponse,
  ProjectPipelineVariableGroupResponse,
  AddProjectPipelineVariableGroupRequest,
  SetProjectTagsRequest,
  SetAgentPoolRequest,
} from '../interfaces/project.interface';
import {
  PushBicepToGitRequest,
  PushBicepToGitResponse,
} from '../interfaces/bicep-generator.interface';
import {
  InfrastructureConfigResponse,
  UserResponse,
  EnvironmentDefinitionResponse,
  ResourceNamingTemplateResponse,
  SetDefaultNamingTemplateRequest,
  SetResourceNamingTemplateRequest,
  ResourceAbbreviationOverrideResponse,
  SetResourceAbbreviationOverrideRequest,
} from '../interfaces/infra-config.interface';
import { ProjectResourceResponse } from '../interfaces/cross-config-reference.interface';
import {
  ProjectRepositoryResponse,
  AddProjectRepositoryRequest,
  UpdateProjectRepositoryRequest,
  ProjectLayoutPreset,
} from '../interfaces/project-repository.interface';
import {
  AddInfraConfigRepositoryRequest,
  ConfigLayoutMode,
  UpdateInfraConfigRepositoryRequest,
} from '../interfaces/infra-config-repository.interface';
import {
  MultiRepoPushRequest,
  MultiRepoPushResponse,
} from '../interfaces/multi-repo-push.interface';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  private readonly axios = inject(AxiosService);

  getMyProjects(): Promise<ProjectResponse[]> {
    return this.axios.request$<ProjectResponse[]>(MethodEnum.GET, '/projects');
  }

  validateRecentItems(request: ValidateRecentItemsRequest): Promise<RecentItemResponse[]> {
    return this.axios.request$<RecentItemResponse[]>(
      MethodEnum.POST,
      '/projects/validate-recent',
      request
    );
  }

  getProject(id: string): Promise<ProjectResponse> {
    return this.axios.request$<ProjectResponse>(MethodEnum.GET, `/projects/${id}`);
  }

  createProject(request: CreateProjectRequest): Promise<ProjectResponse> {
    return this.axios.request$<ProjectResponse>(MethodEnum.POST, '/projects', request);
  }

  createProjectWithSetup(request: CreateProjectWithSetupRequest): Promise<ProjectResponse> {
    return this.axios.request$<ProjectResponse>(MethodEnum.POST, '/projects/with-setup', request);
  }

  deleteProject(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/projects/${id}`);
  }

  getProjectConfigs(id: string): Promise<InfrastructureConfigResponse[]> {
    return this.axios.request$<InfrastructureConfigResponse[]>(
      MethodEnum.GET,
      `/projects/${id}/configs`
    );
  }

  getUsers(): Promise<UserResponse[]> {
    return this.axios.request$<UserResponse[]>(MethodEnum.GET, '/projects/users');
  }

  addMember(projectId: string, request: AddProjectMemberRequest): Promise<ProjectResponse> {
    return this.axios.request$<ProjectResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/members`,
      request
    );
  }

  updateMemberRole(
    projectId: string,
    userId: string,
    request: UpdateProjectMemberRoleRequest
  ): Promise<ProjectResponse> {
    return this.axios.request$<ProjectResponse>(
      MethodEnum.PUT,
      `/projects/${projectId}/members/${userId}`,
      request
    );
  }

  removeMember(projectId: string, userId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/projects/${projectId}/members/${userId}`
    );
  }

  // ─── Environments ───

  addEnvironment(
    projectId: string,
    request: AddProjectEnvironmentRequest
  ): Promise<EnvironmentDefinitionResponse> {
    return this.axios.request$<EnvironmentDefinitionResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/environments`,
      request
    );
  }

  updateEnvironment(
    projectId: string,
    envId: string,
    request: UpdateProjectEnvironmentRequest
  ): Promise<EnvironmentDefinitionResponse> {
    return this.axios.request$<EnvironmentDefinitionResponse>(
      MethodEnum.PUT,
      `/projects/${projectId}/environments/${envId}`,
      request
    );
  }

  removeEnvironment(projectId: string, envId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/projects/${projectId}/environments/${envId}`
    );
  }

  // ─── Naming Templates ───

  setDefaultNamingTemplate(
    projectId: string,
    request: SetDefaultNamingTemplateRequest
  ): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.PUT,
      `/projects/${projectId}/naming/default`,
      request
    );
  }

  setResourceNamingTemplate(
    projectId: string,
    resourceType: string,
    request: SetResourceNamingTemplateRequest
  ): Promise<ResourceNamingTemplateResponse> {
    return this.axios.request$<ResourceNamingTemplateResponse>(
      MethodEnum.PUT,
      `/projects/${projectId}/naming/resources/${resourceType}`,
      request
    );
  }

  removeResourceNamingTemplate(
    projectId: string,
    resourceType: string
  ): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/projects/${projectId}/naming/resources/${resourceType}`
    );
  }

  // ─── Abbreviation Overrides ───

  setResourceAbbreviation(
    projectId: string,
    resourceType: string,
    request: SetResourceAbbreviationOverrideRequest
  ): Promise<ResourceAbbreviationOverrideResponse> {
    return this.axios.request$<ResourceAbbreviationOverrideResponse>(
      MethodEnum.PUT,
      `/projects/${projectId}/naming/abbreviations/${resourceType}`,
      request
    );
  }

  removeResourceAbbreviation(
    projectId: string,
    resourceType: string
  ): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/projects/${projectId}/naming/abbreviations/${resourceType}`
    );
  }

  // ─── Git Operations (test connection + list branches) ───

  testGitConnection(projectId: string): Promise<TestGitConnectionResponse> {
    return this.axios.request$<TestGitConnectionResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/git-config/test`
    );
  }

  listBranches(projectId: string): Promise<GitBranchResponse[]> {
    return this.axios.request$<GitBranchResponse[]>(
      MethodEnum.GET,
      `/projects/${projectId}/git-config/branches`
    );
  }

  // ─── Git Operations (code-specific branches + file search) ───

  listCodeBranches(projectId: string, configId?: string): Promise<GitBranchResponse[]> {
    const params = configId ? `?configId=${configId}` : '';
    return this.axios.request$<GitBranchResponse[]>(
      MethodEnum.GET,
      `/projects/${projectId}/git-config/code-branches${params}`
    );
  }

  searchCodeFiles(projectId: string, branch: string, pattern?: string, configId?: string): Promise<GitFileResponse[]> {
    const params = new URLSearchParams({ branch });
    if (pattern) params.set('pattern', pattern);
    if (configId) params.set('configId', configId);
    return this.axios.request$<GitFileResponse[]>(
      MethodEnum.GET,
      `/projects/${projectId}/git-config/code-files?${params.toString()}`
    );
  }

  // ─── Project Resources ───

  getProjectResources(projectId: string): Promise<ProjectResourceResponse[]> {
    return this.axios.request$<ProjectResourceResponse[]>(
      MethodEnum.GET,
      `/projects/${projectId}/resources`
    );
  }

  generateProjectBicep(projectId: string): Promise<GenerateProjectBicepResponse> {
    return this.axios.request$<GenerateProjectBicepResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/generate-bicep`
    );
  }

  async downloadProjectZip(projectId: string): Promise<Blob> {
    const response = await axios.get(
      `/projects/${projectId}/generate-bicep/download`,
      { responseType: 'blob' }
    );
    return response.data as Blob;
  }

  getProjectBicepFileContent(projectId: string, filePath: string): Promise<string> {
    return this.axios.request$<{ content: string }>(
      MethodEnum.GET,
      `/projects/${projectId}/generate-bicep/files/${filePath}`
    ).then(response => response.content);
  }

  pushProjectBicepToGit(
    projectId: string,
    request: PushBicepToGitRequest
  ): Promise<PushBicepToGitResponse> {
    return this.axios.request$<PushBicepToGitResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/push-to-git`,
      request
    );
  }

  pushProjectGeneratedArtifactsToGit(
    projectId: string,
    request: PushBicepToGitRequest
  ): Promise<PushBicepToGitResponse> {
    return this.axios.request$<PushBicepToGitResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/push-generated-artifacts-to-git`,
      request
    );
  }

  pushProjectArtifactsToMultiRepo(
    projectId: string,
    request: MultiRepoPushRequest
  ): Promise<MultiRepoPushResponse> {
    return this.axios.request$<MultiRepoPushResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/push-multi-repo-artifacts-to-git`,
      request
    );
  }

  // ─── Project Pipeline Generation (mono-repo) ───

  generateProjectPipeline(projectId: string): Promise<GenerateProjectPipelineResponse> {
    return this.axios.request$<GenerateProjectPipelineResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/generate-pipeline`
    );
  }

  async downloadProjectPipelineZip(projectId: string): Promise<Blob> {
    const response = await axios.get(
      `/projects/${projectId}/generate-pipeline/download`,
      { responseType: 'blob' }
    );
    return response.data as Blob;
  }

  getProjectPipelineFileContent(projectId: string, filePath: string): Promise<string> {
    return this.axios.request$<{ content: string }>(
      MethodEnum.GET,
      `/projects/${projectId}/generate-pipeline/files/${filePath}`
    ).then(response => response.content);
  }

  pushProjectPipelineToGit(
    projectId: string,
    request: PushBicepToGitRequest
  ): Promise<PushBicepToGitResponse> {
    return this.axios.request$<PushBicepToGitResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/push-pipeline-to-git`,
      request
    );
  }

  // ─── Project Bootstrap Pipeline Generation (Azure DevOps) ───

  generateProjectBootstrapPipeline(projectId: string): Promise<GenerateProjectBootstrapPipelineResponse> {
    return this.axios.request$<GenerateProjectBootstrapPipelineResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/generate-bootstrap-pipeline`
    );
  }

  async downloadProjectBootstrapPipelineZip(projectId: string): Promise<Blob> {
    const response = await axios.get(
      `/projects/${projectId}/generate-bootstrap-pipeline/download`,
      { responseType: 'blob' }
    );
    return response.data as Blob;
  }

  getProjectBootstrapPipelineFileContent(projectId: string, filePath: string): Promise<string> {
    return this.axios.request$<{ content: string }>(
      MethodEnum.GET,
      `/projects/${projectId}/generate-bootstrap-pipeline/files/${filePath}`
    ).then(response => response.content);
  }

  pushProjectBootstrapPipelineToGit(
    projectId: string,
    request: PushBicepToGitRequest
  ): Promise<PushBicepToGitResponse> {
    return this.axios.request$<PushBicepToGitResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/push-bootstrap-pipeline-to-git`,
      request
    );
  }

  // ─── Pipeline Variable Groups ───

  getPipelineVariableGroups(projectId: string): Promise<ProjectPipelineVariableGroupResponse[]> {
    return this.axios.request$<ProjectPipelineVariableGroupResponse[]>(
      MethodEnum.GET,
      `/projects/${projectId}/pipeline-variable-groups`
    );
  }

  addPipelineVariableGroup(
    projectId: string,
    request: AddProjectPipelineVariableGroupRequest
  ): Promise<ProjectPipelineVariableGroupResponse> {
    return this.axios.request$<ProjectPipelineVariableGroupResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/pipeline-variable-groups`,
      request
    );
  }

  removePipelineVariableGroup(projectId: string, groupId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/projects/${projectId}/pipeline-variable-groups/${groupId}`
    );
  }

  // ─── Tags ───

  setTags(projectId: string, request: SetProjectTagsRequest): Promise<ProjectResponse> {
    return this.axios.request$<ProjectResponse>(
      MethodEnum.PUT,
      `/projects/${projectId}/tags`,
      request
    );
  }

  // ─── Agent Pool ───

  async setAgentPool(projectId: string, request: SetAgentPoolRequest): Promise<void> {
    await this.axios.request$<void>(
      MethodEnum.PUT,
      `/projects/${projectId}/agent-pool`,
      request
    );
  }

  // ─── V2 Multi-Repo Topology ───

  async listRepositories(projectId: string): Promise<ProjectRepositoryResponse[]> {
    const project = await this.getProject(projectId);
    return project.repositories ?? [];
  }

  addRepository(
    projectId: string,
    request: AddProjectRepositoryRequest
  ): Promise<{ id: string }> {
    return this.axios.request$<{ id: string }>(
      MethodEnum.POST,
      `/projects/${projectId}/repositories`,
      request
    );
  }

  updateRepository(
    projectId: string,
    repoId: string,
    request: UpdateProjectRepositoryRequest
  ): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.PUT,
      `/projects/${projectId}/repositories/${repoId}`,
      request
    );
  }

  removeRepository(projectId: string, repoId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/projects/${projectId}/repositories/${repoId}`
    );
  }

  setLayoutPreset(projectId: string, preset: ProjectLayoutPreset): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.PUT,
      `/projects/${projectId}/layout-preset`,
      { preset }
    );
  }

  // ─── Per-config repositories (MultiRepo project layout) ───

  setConfigLayoutMode(
    projectId: string,
    configId: string,
    mode: ConfigLayoutMode | null
  ): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.PUT,
      `/projects/${projectId}/configs/${configId}/layout-mode`,
      { mode }
    );
  }

  addConfigRepository(
    projectId: string,
    configId: string,
    request: AddInfraConfigRepositoryRequest
  ): Promise<{ id: string }> {
    return this.axios.request$<{ id: string }>(
      MethodEnum.POST,
      `/projects/${projectId}/configs/${configId}/repositories`,
      request
    );
  }

  updateConfigRepository(
    projectId: string,
    configId: string,
    repoId: string,
    request: UpdateInfraConfigRepositoryRequest
  ): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.PUT,
      `/projects/${projectId}/configs/${configId}/repositories/${repoId}`,
      request
    );
  }

  removeConfigRepository(
    projectId: string,
    configId: string,
    repoId: string
  ): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/projects/${projectId}/configs/${configId}/repositories/${repoId}`
    );
  }
}

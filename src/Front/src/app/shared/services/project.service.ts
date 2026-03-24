import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  ProjectResponse,
  CreateProjectRequest,
  AddProjectMemberRequest,
  UpdateProjectMemberRoleRequest,
  RecentItemResponse,
  ValidateRecentItemsRequest,
  SetGitConfigRequest,
  TestGitConnectionResponse,
  GitBranchResponse,
} from '../interfaces/project.interface';
import {
  InfrastructureConfigResponse,
  UserResponse,
  EnvironmentDefinitionResponse,
  ResourceNamingTemplateResponse,
  AddEnvironmentRequest,
  UpdateEnvironmentRequest,
  SetDefaultNamingTemplateRequest,
  SetResourceNamingTemplateRequest,
} from '../interfaces/infra-config.interface';

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
    request: AddEnvironmentRequest
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
    request: UpdateEnvironmentRequest
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

  // ─── Git Configuration ───

  setGitConfig(projectId: string, request: SetGitConfigRequest): Promise<ProjectResponse> {
    return this.axios.request$<ProjectResponse>(
      MethodEnum.PUT,
      `/projects/${projectId}/git-config`,
      request
    );
  }

  removeGitConfig(projectId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/projects/${projectId}/git-config`
    );
  }

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
}

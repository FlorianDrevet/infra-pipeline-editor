import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  ProjectResponse,
  CreateProjectRequest,
  AddProjectConfigRequest,
  AddProjectMemberRequest,
  UpdateProjectMemberRoleRequest,
} from '../interfaces/project.interface';
import { InfrastructureConfigResponse } from '../interfaces/infra-config.interface';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  private readonly axios = inject(AxiosService);

  getAll(): Promise<ProjectResponse[]> {
    return this.axios.request$<ProjectResponse[]>(MethodEnum.GET, '/projects');
  }

  getById(id: string): Promise<ProjectResponse> {
    return this.axios.request$<ProjectResponse>(MethodEnum.GET, `/projects/${id}`);
  }

  create(request: CreateProjectRequest): Promise<ProjectResponse> {
    return this.axios.request$<ProjectResponse>(MethodEnum.POST, '/projects', request);
  }

  getConfigurations(projectId: string): Promise<InfrastructureConfigResponse[]> {
    return this.axios.request$<InfrastructureConfigResponse[]>(
      MethodEnum.GET,
      `/projects/${projectId}/configurations`
    );
  }

  addConfiguration(projectId: string, request: AddProjectConfigRequest): Promise<ProjectResponse> {
    return this.axios.request$<ProjectResponse>(
      MethodEnum.POST,
      `/projects/${projectId}/configurations`,
      request
    );
  }

  removeConfiguration(projectId: string, configId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/projects/${projectId}/configurations/${configId}`
    );
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
}

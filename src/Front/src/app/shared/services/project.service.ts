import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  ProjectResponse,
  CreateProjectRequest,
  AddProjectMemberRequest,
  UpdateProjectMemberRoleRequest,
} from '../interfaces/project.interface';
import { InfrastructureConfigResponse } from '../interfaces/infra-config.interface';
import { UserResponse } from '../interfaces/infra-config.interface';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  private readonly axios = inject(AxiosService);

  getMyProjects(): Promise<ProjectResponse[]> {
    return this.axios.request$<ProjectResponse[]>(MethodEnum.GET, '/projects');
  }

  getProject(id: string): Promise<ProjectResponse> {
    return this.axios.request$<ProjectResponse>(MethodEnum.GET, `/projects/${id}`);
  }

  createProject(request: CreateProjectRequest): Promise<ProjectResponse> {
    return this.axios.request$<ProjectResponse>(MethodEnum.POST, '/projects', request);
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
}

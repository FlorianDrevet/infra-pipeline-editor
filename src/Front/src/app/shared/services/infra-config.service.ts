import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  InfrastructureConfigResponse,
  CreateInfrastructureConfigRequest,
  AddEnvironmentRequest,
  UpdateEnvironmentRequest,
  SetDefaultNamingTemplateRequest,
  SetResourceNamingTemplateRequest,
} from '../interfaces/infra-config.interface';
import { ResourceGroupResponse } from '../interfaces/resource-group.interface';

@Injectable({
  providedIn: 'root',
})
export class InfraConfigService {
  private axios = inject(AxiosService);

  getAll(): Promise<InfrastructureConfigResponse[]> {
    return this.axios.request$<InfrastructureConfigResponse[]>(
      MethodEnum.GET,
      '/infra-config'
    );
  }

  getById(id: string): Promise<InfrastructureConfigResponse> {
    return this.axios.request$<InfrastructureConfigResponse>(
      MethodEnum.GET,
      `/infra-config/${id}`
    );
  }

  getResourceGroups(id: string): Promise<ResourceGroupResponse[]> {
    return this.axios.request$<ResourceGroupResponse[]>(
      MethodEnum.GET,
      `/infra-config/${id}/resource-groups`
    );
  }

  create(
    request: CreateInfrastructureConfigRequest
  ): Promise<InfrastructureConfigResponse> {
    return this.axios.request$<InfrastructureConfigResponse>(
      MethodEnum.POST,
      '/infra-config',
      request
    );
  }

  addEnvironment(
    id: string,
    request: AddEnvironmentRequest
  ): Promise<InfrastructureConfigResponse> {
    return this.axios.request$<InfrastructureConfigResponse>(
      MethodEnum.POST,
      `/infra-config/${id}/environments`,
      request
    );
  }

  updateEnvironment(
    id: string,
    envId: string,
    request: UpdateEnvironmentRequest
  ): Promise<InfrastructureConfigResponse> {
    return this.axios.request$<InfrastructureConfigResponse>(
      MethodEnum.PUT,
      `/infra-config/${id}/environments/${envId}`,
      request
    );
  }

  removeEnvironment(id: string, envId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/infra-config/${id}/environments/${envId}`
    );
  }

  setDefaultNamingTemplate(
    id: string,
    request: SetDefaultNamingTemplateRequest
  ): Promise<InfrastructureConfigResponse> {
    return this.axios.request$<InfrastructureConfigResponse>(
      MethodEnum.PUT,
      `/infra-config/${id}/naming/default`,
      request
    );
  }

  setResourceNamingTemplate(
    id: string,
    resourceType: string,
    request: SetResourceNamingTemplateRequest
  ): Promise<InfrastructureConfigResponse> {
    return this.axios.request$<InfrastructureConfigResponse>(
      MethodEnum.PUT,
      `/infra-config/${id}/naming/resources/${resourceType}`,
      request
    );
  }

  removeResourceNamingTemplate(
    id: string,
    resourceType: string
  ): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/infra-config/${id}/naming/resources/${resourceType}`
    );
  }

  setInheritance(
    id: string,
    request: { useProjectEnvironments: boolean; useProjectNamingConventions: boolean }
  ): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.PUT,
      `/infra-config/${id}/inheritance`,
      request
    );
  }
}

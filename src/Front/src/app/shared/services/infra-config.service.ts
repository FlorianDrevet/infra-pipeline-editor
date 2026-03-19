import { inject, Injectable, signal } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  InfrastructureConfigResponse,
  CreateInfrastructureConfigRequest,
  AddMemberRequest,
  UpdateMemberRoleRequest,
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

  // Signals for list view
  configurations = signal<InfrastructureConfigResponse[]>([]);
  isLoading = signal(false);
  error = signal<string | null>(null);

  // Signals for details view
  currentConfig = signal<InfrastructureConfigResponse | null>(null);
  isLoadingDetails = signal(false);

  loadConfigurations(): void {
    this.isLoading.set(true);
    this.error.set(null);
    
    this.getAll()
      .then((configs) => {
        this.configurations.set(configs);
        this.error.set(null);
      })
      .catch((err) => {
        this.error.set(`Failed to load configurations: ${err.message || 'Unknown error'}`);
        this.configurations.set([]);
      })
      .finally(() => this.isLoading.set(false));
  }

  loadConfigDetails(configId: string): void {
    this.isLoadingDetails.set(true);
    
    this.getById(configId)
      .then((config) => {
        this.currentConfig.set(config);
      })
      .catch((err) => {
        console.error('Failed to load config details:', err);
        this.currentConfig.set(null);
      })
      .finally(() => this.isLoadingDetails.set(false));
  }

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

  addMember(id: string, request: AddMemberRequest): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.POST,
      `/infra-config/${id}/members`,
      request
    );
  }

  updateMemberRole(
    id: string,
    userId: string,
    request: UpdateMemberRoleRequest
  ): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.PUT,
      `/infra-config/${id}/members/${userId}`,
      request
    );
  }

  removeMember(id: string, userId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/infra-config/${id}/members/${userId}`
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
}

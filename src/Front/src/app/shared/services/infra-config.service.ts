import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  InfrastructureConfigResponse,
  CreateInfrastructureConfigRequest,
  SetDefaultNamingTemplateRequest,
  SetResourceNamingTemplateRequest,
} from '../interfaces/infra-config.interface';
import { ResourceGroupResponse } from '../interfaces/resource-group.interface';
import {
  CrossConfigReferenceResponse,
  IncomingCrossConfigReferenceResponse,
  AddCrossConfigReferenceRequest,
} from '../interfaces/cross-config-reference.interface';

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

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/infra-config/${id}`);
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
    request: { useProjectNamingConventions: boolean }
  ): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.PUT,
      `/infra-config/${id}/inheritance`,
      request
    );
  }

  // ─── Cross-Config References ───

  getCrossConfigReferences(configId: string): Promise<CrossConfigReferenceResponse[]> {
    return this.axios.request$<CrossConfigReferenceResponse[]>(
      MethodEnum.GET,
      `/infra-config/${configId}/cross-config-references`
    );
  }

  addCrossConfigReference(
    configId: string,
    request: AddCrossConfigReferenceRequest
  ): Promise<CrossConfigReferenceResponse> {
    return this.axios.request$<CrossConfigReferenceResponse>(
      MethodEnum.POST,
      `/infra-config/${configId}/cross-config-references`,
      request
    );
  }

  removeCrossConfigReference(configId: string, referenceId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/infra-config/${configId}/cross-config-references/${referenceId}`
    );
  }

  getIncomingCrossConfigReferences(configId: string): Promise<IncomingCrossConfigReferenceResponse[]> {
    return this.axios.request$<IncomingCrossConfigReferenceResponse[]>(
      MethodEnum.GET,
      `/infra-config/${configId}/incoming-cross-config-references`
    );
  }
}

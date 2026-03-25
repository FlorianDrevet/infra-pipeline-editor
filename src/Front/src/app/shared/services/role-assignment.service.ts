import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  RoleAssignmentResponse,
  AzureRoleDefinitionResponse,
  AddRoleAssignmentRequest,
  UpdateRoleAssignmentIdentityRequest,
} from '../interfaces/role-assignment.interface';

@Injectable({
  providedIn: 'root',
})
export class RoleAssignmentService {
  private axios = inject(AxiosService);

  getByResourceId(resourceId: string): Promise<RoleAssignmentResponse[]> {
    return this.axios.request$<RoleAssignmentResponse[]>(
      MethodEnum.GET,
      `/azure-resources/${resourceId}/role-assignments`
    );
  }

  getAvailableRoleDefinitions(
    resourceId: string
  ): Promise<AzureRoleDefinitionResponse[]> {
    return this.axios.request$<AzureRoleDefinitionResponse[]>(
      MethodEnum.GET,
      `/azure-resources/${resourceId}/role-assignments/available-role-definitions`
    );
  }

  add(
    resourceId: string,
    request: AddRoleAssignmentRequest
  ): Promise<RoleAssignmentResponse> {
    return this.axios.request$<RoleAssignmentResponse>(
      MethodEnum.POST,
      `/azure-resources/${resourceId}/role-assignments`,
      request
    );
  }

  remove(resourceId: string, roleAssignmentId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/azure-resources/${resourceId}/role-assignments/${roleAssignmentId}`
    );
  }

  updateIdentity(
    resourceId: string,
    roleAssignmentId: string,
    request: UpdateRoleAssignmentIdentityRequest
  ): Promise<RoleAssignmentResponse> {
    return this.axios.request$<RoleAssignmentResponse>(
      MethodEnum.PUT,
      `/azure-resources/${resourceId}/role-assignments/${roleAssignmentId}/identity`,
      request
    );
  }
}

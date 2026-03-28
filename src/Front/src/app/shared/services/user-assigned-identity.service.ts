import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  UserAssignedIdentityResponse,
  CreateUserAssignedIdentityRequest,
  UpdateUserAssignedIdentityRequest,
} from '../interfaces/user-assigned-identity.interface';
import { IdentityRoleAssignmentResponse } from '../interfaces/role-assignment.interface';

@Injectable({
  providedIn: 'root',
})
export class UserAssignedIdentityService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<UserAssignedIdentityResponse> {
    return this.axios.request$<UserAssignedIdentityResponse>(
      MethodEnum.GET,
      `/user-assigned-identity/${id}`
    );
  }

  create(request: CreateUserAssignedIdentityRequest): Promise<UserAssignedIdentityResponse> {
    return this.axios.request$<UserAssignedIdentityResponse>(
      MethodEnum.POST,
      '/user-assigned-identity',
      request
    );
  }

  update(id: string, request: UpdateUserAssignedIdentityRequest): Promise<UserAssignedIdentityResponse> {
    return this.axios.request$<UserAssignedIdentityResponse>(
      MethodEnum.PUT,
      `/user-assigned-identity/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/user-assigned-identity/${id}`);
  }

  getGrantedRoleAssignments(id: string): Promise<IdentityRoleAssignmentResponse[]> {
    return this.axios.request$<IdentityRoleAssignmentResponse[]>(
      MethodEnum.GET,
      `/user-assigned-identity/${id}/granted-role-assignments`
    );
  }

  unlinkResource(identityId: string, sourceResourceId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.POST,
      `/user-assigned-identity/${identityId}/unlink-resource`,
      { sourceResourceId }
    );
  }
}

import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  NetworkingProfileResponse,
  SetNetworkingProfileRequest,
} from '../interfaces/networking-profile.interface';

@Injectable({
  providedIn: 'root',
})
export class NetworkingProfileService {
  private readonly axios = inject(AxiosService);

  get(infraConfigId: string): Promise<NetworkingProfileResponse> {
    return this.axios.request$<NetworkingProfileResponse>(
      MethodEnum.GET,
      `/infra-config/${infraConfigId}/networking-profile`
    );
  }

  set(infraConfigId: string, request: SetNetworkingProfileRequest): Promise<NetworkingProfileResponse> {
    return this.axios.request$<NetworkingProfileResponse>(
      MethodEnum.PUT,
      `/infra-config/${infraConfigId}/networking-profile`,
      request
    );
  }

  togglePrivatization(infraConfigId: string, resourceId: string, isPrivatized: boolean): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.PUT,
      `/infra-config/${infraConfigId}/resources/${resourceId}/privatization`,
      { isPrivatized }
    );
  }
}

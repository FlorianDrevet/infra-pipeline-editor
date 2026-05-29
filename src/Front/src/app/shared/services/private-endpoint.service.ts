import { inject, Injectable } from '@angular/core';

import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import { PrivateEndpointConfigResponse, SetPrivateEndpointConfigRequest } from '../interfaces/private-endpoint-config.interface';
import { NetworkingProfileService } from './networking-profile.service';

@Injectable({
  providedIn: 'root',
})
export class PrivateEndpointService {
  private readonly axios = inject(AxiosService);
  private readonly networkingProfileService = inject(NetworkingProfileService);

  toggleResourcePrivatization(infraConfigId: string, resourceId: string, isPrivatized: boolean): Promise<void> {
    return this.networkingProfileService.togglePrivatization(infraConfigId, resourceId, isPrivatized);
  }

  getPrivateEndpointConfig(infraConfigId: string, resourceId: string): Promise<PrivateEndpointConfigResponse | null> {
    return this.axios.request$<PrivateEndpointConfigResponse | null>(
      MethodEnum.GET,
      `/infra-config/${infraConfigId}/resources/${resourceId}/private-endpoint-config`
    );
  }

  setPrivateEndpointConfig(infraConfigId: string, resourceId: string, config: SetPrivateEndpointConfigRequest): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.PUT,
      `/infra-config/${infraConfigId}/resources/${resourceId}/private-endpoint-config`,
      config
    );
  }

  removePrivateEndpointConfig(infraConfigId: string, resourceId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/infra-config/${infraConfigId}/resources/${resourceId}/private-endpoint-config`
    );
  }
}

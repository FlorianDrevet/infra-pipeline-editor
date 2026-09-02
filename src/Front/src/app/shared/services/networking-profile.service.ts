import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';

@Injectable({
  providedIn: 'root',
})
export class NetworkingProfileService {
  private readonly axios = inject(AxiosService);

  togglePrivatization(infraConfigId: string, resourceId: string, isPrivatized: boolean): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.PUT,
      `/infra-config/${infraConfigId}/resources/${resourceId}/privatization`,
      { isPrivatized }
    );
  }
}

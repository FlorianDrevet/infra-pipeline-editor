import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  PersonalAccessTokenResponse,
  CreatedPersonalAccessTokenResponse,
  CreatePersonalAccessTokenRequest,
} from '../interfaces/personal-access-token.interface';

@Injectable({
  providedIn: 'root',
})
export class PersonalAccessTokenService {
  private readonly axios = inject(AxiosService);

  getAll(): Promise<PersonalAccessTokenResponse[]> {
    return this.axios.request$<PersonalAccessTokenResponse[]>(MethodEnum.GET, '/personal-access-tokens');
  }

  create(request: CreatePersonalAccessTokenRequest): Promise<CreatedPersonalAccessTokenResponse> {
    return this.axios.request$<CreatedPersonalAccessTokenResponse>(MethodEnum.POST, '/personal-access-tokens', request);
  }

  revoke(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/personal-access-tokens/${id}`);
  }
}

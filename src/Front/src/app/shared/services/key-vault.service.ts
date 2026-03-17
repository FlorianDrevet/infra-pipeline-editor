import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  KeyVaultResponse,
  CreateKeyVaultRequest,
  UpdateKeyVaultRequest,
} from '../interfaces/key-vault.interface';

@Injectable({
  providedIn: 'root',
})
export class KeyVaultService {
  private axios = inject(AxiosService);

  getById(id: string): Promise<KeyVaultResponse> {
    return this.axios.request$<KeyVaultResponse>(
      MethodEnum.GET,
      `/keyvault/${id}`
    );
  }

  create(request: CreateKeyVaultRequest): Promise<KeyVaultResponse> {
    return this.axios.request$<KeyVaultResponse>(
      MethodEnum.POST,
      '/keyvault',
      request
    );
  }

  update(id: string, request: UpdateKeyVaultRequest): Promise<KeyVaultResponse> {
    return this.axios.request$<KeyVaultResponse>(
      MethodEnum.PUT,
      `/keyvault/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/keyvault/${id}`);
  }
}

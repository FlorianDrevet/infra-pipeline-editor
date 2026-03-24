import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  CosmosDbResponse,
  CreateCosmosDbRequest,
  UpdateCosmosDbRequest,
} from '../interfaces/cosmos-db.interface';

@Injectable({
  providedIn: 'root',
})
export class CosmosDbService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<CosmosDbResponse> {
    return this.axios.request$<CosmosDbResponse>(
      MethodEnum.GET,
      `/cosmos-db/${id}`
    );
  }

  create(request: CreateCosmosDbRequest): Promise<CosmosDbResponse> {
    return this.axios.request$<CosmosDbResponse>(
      MethodEnum.POST,
      '/cosmos-db',
      request
    );
  }

  update(id: string, request: UpdateCosmosDbRequest): Promise<CosmosDbResponse> {
    return this.axios.request$<CosmosDbResponse>(
      MethodEnum.PUT,
      `/cosmos-db/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/cosmos-db/${id}`);
  }
}

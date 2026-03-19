import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  RedisCacheResponse,
  CreateRedisCacheRequest,
  UpdateRedisCacheRequest,
} from '../interfaces/redis-cache.interface';

@Injectable({
  providedIn: 'root',
})
export class RedisCacheService {
  private axios = inject(AxiosService);

  getById(id: string): Promise<RedisCacheResponse> {
    return this.axios.request$<RedisCacheResponse>(
      MethodEnum.GET,
      `/redis-cache/${id}`
    );
  }

  create(request: CreateRedisCacheRequest): Promise<RedisCacheResponse> {
    return this.axios.request$<RedisCacheResponse>(
      MethodEnum.POST,
      '/redis-cache',
      request
    );
  }

  update(
    id: string,
    request: UpdateRedisCacheRequest
  ): Promise<RedisCacheResponse> {
    return this.axios.request$<RedisCacheResponse>(
      MethodEnum.PUT,
      `/redis-cache/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/redis-cache/${id}`);
  }
}

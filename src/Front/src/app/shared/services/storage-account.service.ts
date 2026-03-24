import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  StorageAccountResponse,
  CreateStorageAccountRequest,
  UpdateStorageAccountRequest,
  AddBlobContainerRequest,
  AddQueueRequest,
  AddTableRequest,
  UpdateBlobContainerPublicAccessRequest,
} from '../interfaces/storage-account.interface';

@Injectable({
  providedIn: 'root',
})
export class StorageAccountService {
  private axios = inject(AxiosService);

  getById(id: string): Promise<StorageAccountResponse> {
    return this.axios.request$<StorageAccountResponse>(
      MethodEnum.GET,
      `/storage-accounts/${id}`
    );
  }

  create(
    request: CreateStorageAccountRequest
  ): Promise<StorageAccountResponse> {
    return this.axios.request$<StorageAccountResponse>(
      MethodEnum.POST,
      '/storage-accounts',
      request
    );
  }

  update(
    id: string,
    request: UpdateStorageAccountRequest
  ): Promise<StorageAccountResponse> {
    return this.axios.request$<StorageAccountResponse>(
      MethodEnum.PUT,
      `/storage-accounts/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/storage-accounts/${id}`
    );
  }

  addBlobContainer(
    id: string,
    request: AddBlobContainerRequest
  ): Promise<StorageAccountResponse> {
    return this.axios.request$<StorageAccountResponse>(
      MethodEnum.POST,
      `/storage-accounts/${id}/blob-containers`,
      request
    );
  }

  removeBlobContainer(id: string, containerId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/storage-accounts/${id}/blob-containers/${containerId}`
    );
  }

  updateBlobContainerPublicAccess(
    id: string,
    containerId: string,
    request: UpdateBlobContainerPublicAccessRequest
  ): Promise<StorageAccountResponse> {
    return this.axios.request$<StorageAccountResponse>(
      MethodEnum.PUT,
      `/storage-accounts/${id}/blob-containers/${containerId}`,
      request
    );
  }

  addQueue(
    id: string,
    request: AddQueueRequest
  ): Promise<StorageAccountResponse> {
    return this.axios.request$<StorageAccountResponse>(
      MethodEnum.POST,
      `/storage-accounts/${id}/queues`,
      request
    );
  }

  removeQueue(id: string, queueId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/storage-accounts/${id}/queues/${queueId}`
    );
  }

  addTable(
    id: string,
    request: AddTableRequest
  ): Promise<StorageAccountResponse> {
    return this.axios.request$<StorageAccountResponse>(
      MethodEnum.POST,
      `/storage-accounts/${id}/tables`,
      request
    );
  }

  removeTable(id: string, tableId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/storage-accounts/${id}/tables/${tableId}`
    );
  }
}

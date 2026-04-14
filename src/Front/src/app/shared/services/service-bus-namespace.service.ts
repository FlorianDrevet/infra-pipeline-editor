import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  ServiceBusNamespaceResponse,
  CreateServiceBusNamespaceRequest,
  UpdateServiceBusNamespaceRequest,
  AddServiceBusQueueRequest,
  AddServiceBusTopicSubscriptionRequest,
} from '../interfaces/service-bus-namespace.interface';

@Injectable({
  providedIn: 'root',
})
export class ServiceBusNamespaceService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<ServiceBusNamespaceResponse> {
    return this.axios.request$<ServiceBusNamespaceResponse>(
      MethodEnum.GET,
      `/service-bus/${id}`
    );
  }

  create(request: CreateServiceBusNamespaceRequest): Promise<ServiceBusNamespaceResponse> {
    return this.axios.request$<ServiceBusNamespaceResponse>(
      MethodEnum.POST,
      '/service-bus',
      request
    );
  }

  update(id: string, request: UpdateServiceBusNamespaceRequest): Promise<ServiceBusNamespaceResponse> {
    return this.axios.request$<ServiceBusNamespaceResponse>(
      MethodEnum.PUT,
      `/service-bus/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/service-bus/${id}`
    );
  }

  addQueue(id: string, request: AddServiceBusQueueRequest): Promise<ServiceBusNamespaceResponse> {
    return this.axios.request$<ServiceBusNamespaceResponse>(
      MethodEnum.POST,
      `/service-bus/${id}/queues`,
      request
    );
  }

  removeQueue(id: string, queueId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/service-bus/${id}/queues/${queueId}`
    );
  }

  addTopicSubscription(id: string, request: AddServiceBusTopicSubscriptionRequest): Promise<ServiceBusNamespaceResponse> {
    return this.axios.request$<ServiceBusNamespaceResponse>(
      MethodEnum.POST,
      `/service-bus/${id}/topic-subscriptions`,
      request
    );
  }

  removeTopicSubscription(id: string, subscriptionId: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/service-bus/${id}/topic-subscriptions/${subscriptionId}`
    );
  }
}

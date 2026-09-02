import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  DocumentIntelligenceResponse,
  CreateDocumentIntelligenceRequest,
  UpdateDocumentIntelligenceRequest,
} from '../interfaces/document-intelligence.interface';

@Injectable({
  providedIn: 'root',
})
export class DocumentIntelligenceService {
  private readonly axios = inject(AxiosService);

  getById(id: string): Promise<DocumentIntelligenceResponse> {
    return this.axios.request$<DocumentIntelligenceResponse>(
      MethodEnum.GET,
      `/document-intelligence/${id}`
    );
  }

  create(request: CreateDocumentIntelligenceRequest): Promise<DocumentIntelligenceResponse> {
    return this.axios.request$<DocumentIntelligenceResponse>(
      MethodEnum.POST,
      '/document-intelligence',
      request
    );
  }

  update(id: string, request: UpdateDocumentIntelligenceRequest): Promise<DocumentIntelligenceResponse> {
    return this.axios.request$<DocumentIntelligenceResponse>(
      MethodEnum.PUT,
      `/document-intelligence/${id}`,
      request
    );
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(
      MethodEnum.DELETE,
      `/document-intelligence/${id}`
    );
  }
}

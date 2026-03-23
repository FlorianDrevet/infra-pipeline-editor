import { Injectable } from '@angular/core';
import axios from 'axios';
import {
  GenerateBicepRequest,
  GenerateBicepResponse,
} from '../interfaces/bicep-generator.interface';

@Injectable({
  providedIn: 'root',
})
export class BicepGeneratorService {
  async generate(request: GenerateBicepRequest): Promise<GenerateBicepResponse> {
    const response = await axios.post<GenerateBicepResponse>(
      '/generate-bicep',
      request,
    );
    return response.data;
  }

  async downloadZip(configId: string): Promise<Blob> {
    const response = await axios.get(
      `/generate-bicep/${configId}/download`,
      { responseType: 'blob' },
    );
    return response.data as Blob;
  }

  async getFileContent(configId: string, filePath: string): Promise<string> {
    const response = await axios.get<{ content: string }>(
      `/generate-bicep/${configId}/files/${filePath}`,
    );
    return response.data.content;
  }
}
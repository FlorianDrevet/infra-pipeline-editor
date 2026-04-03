import { Injectable } from '@angular/core';
import axios from 'axios';
import {
  GeneratePipelineRequest,
  GeneratePipelineResponse,
  GenerateAppPipelineRequest,
  GenerateAppPipelineResponse,
  PushPipelineToGitRequest,
  PushPipelineToGitResponse,
} from '../interfaces/pipeline-generator.interface';

@Injectable({
  providedIn: 'root',
})
export class PipelineGeneratorService {
  async generate(request: GeneratePipelineRequest): Promise<GeneratePipelineResponse> {
    const response = await axios.post<GeneratePipelineResponse>(
      '/generate-pipeline',
      request,
    );
    return response.data;
  }

  async generateAppPipeline(request: GenerateAppPipelineRequest): Promise<GenerateAppPipelineResponse> {
    const response = await axios.post<GenerateAppPipelineResponse>(
      '/generate-pipeline/app',
      request,
    );
    return response.data;
  }

  async downloadZip(configId: string): Promise<Blob> {
    const response = await axios.get(
      `/generate-pipeline/${configId}/download`,
      { responseType: 'blob' },
    );
    return response.data as Blob;
  }

  async getFileContent(configId: string, filePath: string): Promise<string> {
    const response = await axios.get<{ content: string }>(
      `/generate-pipeline/${configId}/files/${filePath}`,
    );
    return response.data.content;
  }

  async pushToGit(configId: string, request: PushPipelineToGitRequest): Promise<PushPipelineToGitResponse> {
    const response = await axios.post<PushPipelineToGitResponse>(
      `/generate-pipeline/${configId}/push-to-git`,
      request,
    );
    return response.data;
  }
}

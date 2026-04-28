import { Injectable } from '@angular/core';
import {
  GeneratePipelineRequest,
  GeneratePipelineResponse,
  PushPipelineToGitRequest,
  PushPipelineToGitResponse,
} from '../interfaces/pipeline-generator.interface';
import { BaseGeneratorService } from './base-generator.service';

@Injectable({
  providedIn: 'root',
})
export class PipelineGeneratorService extends BaseGeneratorService<
  GeneratePipelineRequest, GeneratePipelineResponse, PushPipelineToGitRequest, PushPipelineToGitResponse
> {
  protected readonly basePath = '/generate-pipeline';
}

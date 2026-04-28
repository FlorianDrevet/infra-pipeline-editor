import { Injectable } from '@angular/core';
import {
  GenerateBicepRequest,
  GenerateBicepResponse,
  PushBicepToGitRequest,
  PushBicepToGitResponse,
} from '../interfaces/bicep-generator.interface';
import { BaseGeneratorService } from './base-generator.service';

@Injectable({
  providedIn: 'root',
})
export class BicepGeneratorService extends BaseGeneratorService<
  GenerateBicepRequest, GenerateBicepResponse, PushBicepToGitRequest, PushBicepToGitResponse
> {
  protected readonly basePath = '/generate-bicep';
}
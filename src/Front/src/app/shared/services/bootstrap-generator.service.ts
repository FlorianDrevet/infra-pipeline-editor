import { Injectable } from '@angular/core';
import {
  GenerateBootstrapRequest,
  GenerateBootstrapResponse,
  PushBootstrapToGitRequest,
  PushBootstrapToGitResponse,
} from '../interfaces/bootstrap-generator.interface';
import { BaseGeneratorService } from './base-generator.service';

@Injectable({
  providedIn: 'root',
})
export class BootstrapGeneratorService extends BaseGeneratorService<
  GenerateBootstrapRequest, GenerateBootstrapResponse, PushBootstrapToGitRequest, PushBootstrapToGitResponse
> {
  protected readonly basePath = '/generate-bootstrap';
}

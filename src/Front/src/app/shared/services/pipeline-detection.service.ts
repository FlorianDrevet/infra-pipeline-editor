import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import { DetectedPipelineOptionsResponse } from '../interfaces/pipeline-detection.interface';

@Injectable({
  providedIn: 'root',
})
export class PipelineDetectionService {
  private readonly axios = inject(AxiosService);

  detect(resourceId: string): Promise<DetectedPipelineOptionsResponse> {
    return this.axios.request$<DetectedPipelineOptionsResponse>(
      MethodEnum.GET,
      `/azure-resources/${resourceId}/detect-pipeline-options`
    );
  }
}

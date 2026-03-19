import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { MethodEnum } from '../enums/method.enum';
import {
  GenerateBicepRequest,
  GenerateBicepResponse,
} from '../interfaces/bicep-generator.interface';
import { AxiosService } from './axios.service';

@Injectable({
  providedIn: 'root',
})
export class BicepGeneratorService {
  private readonly axios = inject(AxiosService);

  generate(request: GenerateBicepRequest): Promise<GenerateBicepResponse> {
    return this.axios.request$<GenerateBicepResponse>(
      MethodEnum.POST,
      `${environment.bicep_api_url}/generate-bicep`,
      request
    );
  }
}
import { inject, Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';
import {
  GenerateBicepRequest,
  GenerateBicepResponse,
} from '../interfaces/bicep-generator.interface';
import { MsalAuthService } from './msal-auth.service';

const bicepAxios = axios.create();

@Injectable({
  providedIn: 'root',
})
export class BicepGeneratorService {
  private readonly msalAuth = inject(MsalAuthService);
  private readonly bicepApiScopes = environment.msalConfig?.bicepApiScopes ?? [];

  async generate(request: GenerateBicepRequest): Promise<GenerateBicepResponse> {
    const token = await this.msalAuth.getAccessTokenForScopes(this.bicepApiScopes);
    if (!token) {
      throw new Error('No access token for Bicep API. Please sign in.');
    }

    const response = await bicepAxios.post<GenerateBicepResponse>(
      `${environment.bicep_api_url}/generate-bicep`,
      request,
      {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      }
    );

    return response.data;
  }
}
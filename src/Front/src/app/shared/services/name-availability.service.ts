import { Injectable, inject } from '@angular/core';
import { Observable, from } from 'rxjs';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import {
  CheckResourceNameAvailabilityRequest,
  CheckResourceNameAvailabilityResponse,
} from '../interfaces/name-availability.interface';

@Injectable({ providedIn: 'root' })
export class NameAvailabilityService {
  private readonly axios = inject(AxiosService);

  /** Checks the availability of a resource name across all environments of the project. */
  check$(
    resourceType: string,
    body: CheckResourceNameAvailabilityRequest,
  ): Observable<CheckResourceNameAvailabilityResponse> {
    return from(
      this.axios.request$<CheckResourceNameAvailabilityResponse>(
        MethodEnum.POST,
        `/naming/check-availability/${resourceType}`,
        body,
      ),
    );
  }
}

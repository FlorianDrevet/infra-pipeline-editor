import { inject, Injectable } from '@angular/core';
import { AxiosService } from '../services/axios.service';
import { MethodEnum } from '../enums/method.enum';
import { AuthTokenInterface } from '../interfaces/authToken.interface';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationFacadeService {
  axiosService = inject(AxiosService);

  public postLogIn$(username: string, password: string): Promise<AuthTokenInterface> {
    return this.axiosService.request$<AuthTokenInterface>(MethodEnum.POST, `/auth/login`, {
      email: username,
      password,
    });
  }
}

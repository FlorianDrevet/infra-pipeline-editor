import { Routes } from '@angular/router';
import { AuthenticationGuard } from './shared/guards/authentication.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    canActivate: [AuthenticationGuard],
    children: [
      {
        path: 'configs',
        loadComponent: () =>
          import('./features/infrastructure-configs/pages/configs-list.component').then(
            (m) => m.ConfigsListComponent
          ),
      },
      {
        path: 'configs/:id',
        loadComponent: () =>
          import('./features/infrastructure-configs/pages/config-details.component').then(
            (m) => m.ConfigDetailsComponent
          ),
      },
      {
        path: '',
        redirectTo: 'configs',
        pathMatch: 'full',
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];

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
        path: 'infrastructure-configs',
        loadComponent: () =>
          import('./features/infrastructure-configs/pages/configs-list/configs-list.component').then(
            (m) => m.ConfigsListComponent
          ),
      },
      {
        path: 'infrastructure-configs/:id',
        loadComponent: () =>
          import('./features/infrastructure-configs/pages/config-details/config-details.component').then(
            (m) => m.ConfigDetailsComponent
          ),
      },
      {
        path: 'configs',
        redirectTo: 'infrastructure-configs',
        pathMatch: 'full',
      },
      {
        path: 'configs/:id',
        redirectTo: 'infrastructure-configs/:id',
        pathMatch: 'full',
      },
      {
        path: '',
        redirectTo: 'infrastructure-configs',
        pathMatch: 'full',
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];

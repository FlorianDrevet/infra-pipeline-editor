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
        path: '',
        loadComponent: () =>
          import('./features/home/home.component').then((m) => m.HomeComponent),
      },
      {
        path: 'config/:id',
        loadComponent: () =>
          import('./features/config-detail/config-detail.component').then(
            (m) => m.ConfigDetailComponent
          ),
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];

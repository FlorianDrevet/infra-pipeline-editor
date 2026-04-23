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
        path: 'projects',
        loadComponent: () =>
          import('./features/projects/projects.component').then(
            (m) => m.ProjectsComponent
          ),
      },
      {
        path: 'projects/:id',
        loadComponent: () =>
          import('./features/project-detail/project-detail.component').then(
            (m) => m.ProjectDetailComponent
          ),
      },
      {
        path: 'config/:id',
        loadComponent: () =>
          import('./features/config-detail/config-detail.component').then(
            (m) => m.ConfigDetailComponent
          ),
      },
      {
        path: 'config/:configId/resource/:resourceType/:resourceId',
        loadComponent: () =>
          import('./features/resource-edit/resource-edit.component').then(
            (m) => m.ResourceEditComponent
          ),
      },
      {
        path: 'design-system',
        loadComponent: () =>
          import('./features/design-system/design-system.component').then(
            (m) => m.DesignSystemComponent
          ),
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];

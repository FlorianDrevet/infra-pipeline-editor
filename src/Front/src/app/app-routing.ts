import { Route, Routes } from '@angular/router';
import { AuthenticationGuard } from './shared/guards/authentication.guard';
import { environment } from '../environments/environment';

const devOnlyRoutes: Route[] = environment.production
  ? []
  : [
      {
        path: 'design-system',
        loadComponent: () =>
          import('./features/design-system/design-system.component').then(
            (m) => m.DesignSystemComponent
          ),
      },
    ];

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
        path: 'projects/:id/members',
        loadComponent: () =>
          import('./features/project-members/project-members.component').then(
            (m) => m.ProjectMembersComponent
          ),
      },
      {
        path: 'projects/:id/settings',
        loadComponent: () =>
          import('./features/project-settings/project-settings.component').then(
            (m) => m.ProjectSettingsComponent
          ),
      },
      {
        path: 'projects/:id/generate/config',
        loadComponent: () =>
          import('./features/project-detail/generation-config/generation-config.component').then(
            (m) => m.GenerationConfigComponent
          ),
      },
      {
        path: 'projects/:id/generate',
        loadComponent: () =>
          import('./features/project-detail/generation-board/generation-board.component').then(
            (m) => m.GenerationBoardComponent
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
        path: 'config/:id/generate',
        loadComponent: () =>
          import('./features/config-detail/config-generation/config-generation.component').then(
            (m) => m.ConfigGenerationComponent
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
        path: 'settings',
        loadComponent: () =>
          import('./features/settings/settings.component').then(
            (m) => m.SettingsComponent
          ),
      },
      ...devOnlyRoutes,
    ],
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];

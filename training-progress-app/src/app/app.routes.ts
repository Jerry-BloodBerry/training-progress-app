import { Routes } from '@angular/router';
import { LandingComponent } from './landing/landing.component';
import { canActivateClerk } from 'ngx-clerk';

export const routes: Routes = [
  { path: '', component: LandingComponent },
  {
    path: 'dashboard',
    canActivate: [canActivateClerk],
    loadComponent: () =>
      import('./dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  {
    path: 'trainings',
    canActivate: [canActivateClerk],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./trainings/trainings.component').then((m) => m.TrainingsComponent),
      },
      {
        path: 'new',
        loadComponent: () =>
          import('./trainings/training-form.component').then((m) => m.TrainingFormComponent),
      },
      {
        path: ':id',
        loadComponent: () =>
          import('./trainings/training-detail.component').then((m) => m.TrainingDetailComponent),
      },
      {
        path: ':id/edit',
        loadComponent: () =>
          import('./trainings/training-form.component').then((m) => m.TrainingFormComponent),
      },
    ],
  },
];

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
];

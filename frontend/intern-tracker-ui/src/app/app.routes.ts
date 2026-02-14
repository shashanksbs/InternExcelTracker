import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';
import { AdminDashboardComponent } from './admin/dashboard/dashboard.component';
import { InternDashboardComponent } from './intern/dashboard/dashboard.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { 
    path: 'admin/dashboard', 
    component: AdminDashboardComponent, 
    canActivate: [authGuard],
    data: { role: 'Admin' }
  },
  { 
    path: 'intern/dashboard', 
    component: InternDashboardComponent, 
    canActivate: [authGuard], 
    data: { role: 'Intern' }
  },
  { path: '**', redirectTo: 'login' }
];

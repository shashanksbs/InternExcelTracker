import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { ToastService } from './toast.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private toastService = inject(ToastService);
  private apiUrl = `${environment.apiUrl}/auth`;
  private USER_KEY = 'intern_tracker_user';
  private LAST_URL_KEY = 'last_visited_url';

  constructor() { }

  register(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  login(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, data).pipe(
      tap((res: any) => {
        if (res) {
          localStorage.setItem(this.USER_KEY, JSON.stringify(res));
          this.toastService.success('Login successful');
          this.redirectBasedOnRole(res.role);
        }
      })
    );
  }

  logout() {
    localStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.LAST_URL_KEY);
    this.router.navigate(['/login']);
  }

  getUser(): any {
    const userStr = localStorage.getItem(this.USER_KEY);
    return userStr ? JSON.parse(userStr) : null;
  }

  isLoggedIn(): boolean {
    return !!this.getUser();
  }

  isAdmin(): boolean {
    const user = this.getUser();
    return user && user.role === 'Admin';
  }

  isIntern(): boolean {
    const user = this.getUser();
    return user && user.role === 'Intern';
  }

  private redirectBasedOnRole(role: string) {
    const lastUrl = localStorage.getItem(this.LAST_URL_KEY);
    if (lastUrl && lastUrl !== '/login' && lastUrl !== '/register' && lastUrl !== '/') {
      this.router.navigateByUrl(lastUrl);
    } else {
      if (role === 'Admin') {
        this.router.navigate(['/admin/dashboard']);
      } else {
        this.router.navigate(['/intern/dashboard']);
      }
    }
  }
}

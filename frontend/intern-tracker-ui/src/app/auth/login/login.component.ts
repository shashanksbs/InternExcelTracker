import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css' // Assuming you have or will have a css file, sticking to standard structure. If not exists, might error, but assuming standard component structure.
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loginForm = this.fb.group({
    username: ['', Validators.required],
    password: ['', Validators.required],
    role: ['Intern', Validators.required]
  });

  errorMessage = '';

  ngOnInit() {
    if (this.authService.isLoggedIn()) {
      const user = this.authService.getUser();
      const lastUrl = localStorage.getItem('last_visited_url');
      
      if (lastUrl && lastUrl !== '/login' && lastUrl !== '/register' && lastUrl !== '/') {
        this.router.navigateByUrl(lastUrl);
      } else {
        if (user.role === 'Admin') {
          this.router.navigate(['/admin/dashboard']);
        } else {
          this.router.navigate(['/intern/dashboard']);
        }
      }
    }
  }

  onSubmit() {
    if (this.loginForm.valid) {
      this.authService.login(this.loginForm.value).subscribe({
        next: () => {
          // Redirect handled in service
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Login failed';
        }
      });
    }
  }
}

import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  role: 'Member' | 'Admin' = 'Member';

  readonly isRegisterMode = signal(false);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showPassword = signal(false);

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      this.router.navigate(['/documents']);
    }
  }

  toggleMode(): void {
    this.isRegisterMode.update(v => !v);
    this.errorMessage.set(null);
  }

  setMode(isRegister: boolean): void {
    if (this.isRegisterMode() !== isRegister) {
      this.isRegisterMode.set(isRegister);
      this.errorMessage.set(null);
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(v => !v);
  }

  onSubmit(): void {
    const trimmedEmail = this.email.trim();
    if (!trimmedEmail || !this.password) {
      this.errorMessage.set('Please enter both your email address and password.');
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(trimmedEmail)) {
      this.errorMessage.set('Please enter a valid email address.');
      return;
    }

    if (this.isRegisterMode() && this.password.length < 6) {
      this.errorMessage.set('Password must be at least 6 characters long.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const action$ = this.isRegisterMode()
      ? this.auth.register({ email: trimmedEmail, password: this.password, role: this.role })
      : this.auth.login({ email: trimmedEmail, password: this.password });

    action$.subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/documents']);
      },
      error: (err) => {
        this.isLoading.set(false);
        const serverMsg = err?.error?.message;
        if (err.status === 0) {
          this.errorMessage.set('Unable to reach the server. Please verify the backend is running.');
        } else if (serverMsg) {
          this.errorMessage.set(serverMsg);
        } else {
          this.errorMessage.set(
            this.isRegisterMode()
              ? 'Registration failed. Please check your information and try again.'
              : 'Invalid email or password. Please try again.'
          );
        }
      }
    });
  }
}

import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

export interface AuthSession {
  token: string;
  email: string;
  role: 'Admin' | 'Member';
  userId: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly session = signal<AuthSession | null>(this.loadInitialSession());

  readonly isAuthenticated = computed(() => !!this.session()?.token);
  readonly userRole = computed(() => this.session()?.role ?? null);
  readonly isAdmin = computed(() => this.userRole() === 'Admin');
  readonly userEmail = computed(() => this.session()?.email ?? '');

  login(credentials: { email: string; password: string }): Observable<AuthSession> {
    return this.http.post<AuthSession>('/api/auth/login', credentials).pipe(
      tap((data) => this.setSession(data))
    );
  }

  register(payload: { email: string; password: string; role?: string }): Observable<AuthSession> {
    return this.http.post<AuthSession>('/api/auth/register', payload).pipe(
      tap((data) => this.setSession(data))
    );
  }

  logout(): void {
    localStorage.removeItem('aiw_session');
    this.session.set(null);
    this.router.navigate(['/login']);
  }

  private setSession(data: AuthSession): void {
    localStorage.setItem('aiw_session', JSON.stringify(data));
    this.session.set(data);
  }

  private loadInitialSession(): AuthSession | null {
    const raw = localStorage.getItem('aiw_session');
    return raw ? JSON.parse(raw) : null;
  }
}
import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { User, LoginRequest, RegisterRequest, AuthResponse } from '../../models/api.models';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  
  private readonly API_URL = environment.apiUrl;
  private readonly TOKEN_KEY = 'silkroad_token';
  private readonly USER_KEY = 'silkroad_user';

  // Signals for state management
  private _user = signal<User | null>(this.loadUserFromStorage());
  private _token = signal<string | null>(this.loadTokenFromStorage());
  private _isLoading = signal(false);
  private _error = signal<string | null>(null);

  // Public readonly signals
  readonly user = this._user.asReadonly();
  readonly token = this._token.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly isAuthenticated = computed(() => !!this._token() && !!this._user());

  private loadTokenFromStorage(): string | null {
    if (typeof localStorage !== 'undefined') {
      return localStorage.getItem(this.TOKEN_KEY);
    }
    return null;
  }

  private loadUserFromStorage(): User | null {
    if (typeof localStorage !== 'undefined') {
      const userStr = localStorage.getItem(this.USER_KEY);
      return userStr ? JSON.parse(userStr) : null;
    }
    return null;
  }

  private saveToStorage(token: string, user: User): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  private clearStorage(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    localStorage.removeItem('currentStoreId');
  }

  login(credentials: LoginRequest): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.post<AuthResponse>(`${this.API_URL}/auth/login`, credentials)
      .pipe(
        tap(response => {
          // Backend returns { id, email, fullName, token, tokenExpiration }
          const user: User = {
            id: response.id,
            email: response.email,
            fullName: response.fullName
          };
          this._token.set(response.token);
          this._user.set(user);
          this.saveToStorage(response.token, user);
          this._isLoading.set(false);
          this.router.navigate(['/home']);
        }),
        catchError(error => {
          this._error.set(error.error?.message || 'Login failed. Please check your credentials.');
          this._isLoading.set(false);
          return of(null);
        })
      )
      .subscribe();
  }

  register(data: RegisterRequest): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.post<AuthResponse>(`${this.API_URL}/auth/register`, data)
      .pipe(
        tap(response => {
          // Backend returns { id, email, fullName, token, tokenExpiration }
          const user: User = {
            id: response.id,
            email: response.email,
            fullName: response.fullName
          };
          this._token.set(response.token);
          this._user.set(user);
          this.saveToStorage(response.token, user);
          this._isLoading.set(false);
          this.router.navigate(['/home']);
        }),
        catchError(error => {
          this._error.set(error.error?.message || 'Registration failed. Please try again.');
          this._isLoading.set(false);
          return of(null);
        })
      )
      .subscribe();
  }

  logout(): void {
    this._token.set(null);
    this._user.set(null);
    this.clearStorage();
    this.router.navigate(['/auth/login']);
  }

  clearError(): void {
    this._error.set(null);
  }
}

import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, catchError, finalize } from 'rxjs/operators';
import { SocialPlatform } from '../../../core/models/api.models';
import { environment } from '../../../../environments/environment';

export interface AvailablePlatform {
  value: number;
  name: string;
  displayName: string;
  isOAuthEnabled: boolean;
}

export interface ConnectFacebookRequest {
  code: string;
  redirectUri: string;
}

@Injectable({
  providedIn: 'root'
})
export class PlatformsService {
  private http = inject(HttpClient);
  private readonly API_URL = `${environment.apiUrl}/social-platforms`;

  // State signals
  private _platforms = signal<SocialPlatform[]>([]);
  private _availablePlatforms = signal<AvailablePlatform[]>([]);
  private _isLoading = signal(false);
  private _error = signal<string | null>(null);

  // Public readonly signals
  readonly platforms = this._platforms.asReadonly();
  readonly availablePlatforms = this._availablePlatforms.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();

  // Computed signals
  readonly connectedPlatforms = computed(() => 
    this._platforms().filter(p => p.isConnected)
  );

  readonly hasFacebookConnected = computed(() =>
    this._platforms().some(p => p.platformName === 'Facebook' && p.isConnected)
  );

  /**
   * Load available platforms for dropdown
   * GET /api/social-platforms/available-platforms (GLOBAL - no X-Store-ID)
   */
  loadAvailablePlatforms(): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<AvailablePlatform[]>(`${this.API_URL}/available-platforms`)
      .pipe(
        tap(platforms => this._availablePlatforms.set(platforms)),
        catchError(err => {
          this._error.set(err.error?.message || 'Failed to load available platforms');
          return of([]);
        }),
        finalize(() => this._isLoading.set(false))
      )
      .subscribe();
  }

  /**
   * Load connected platforms for current store
   * GET /api/social-platforms (STORE-SCOPED)
   */
  loadConnectedPlatforms(): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<SocialPlatform[]>(`${this.API_URL}`)
      .pipe(
        tap(platforms => {
          console.log('Loaded connected platforms:', platforms);
          this._platforms.set(platforms);
        }),
        catchError(err => {
          console.error('Failed to load connected platforms:', err);
          this._error.set(err.error?.message || 'Failed to load connected platforms');
          return of([]);
        }),
        finalize(() => this._isLoading.set(false))
      )
      .subscribe();
  }

  /**
   * Get connected platforms (returns Observable)
   * GET /api/social-platforms (STORE-SCOPED)
   */
  getConnectedPlatforms(): Observable<SocialPlatform[]> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.get<SocialPlatform[]>(`${this.API_URL}`).pipe(
      tap(platforms => {
        console.log('Fetched connected platforms:', platforms);
        this._platforms.set(platforms);
      }),
      catchError(err => {
        console.error('Error fetching connected platforms:', err);
        this._error.set(err.error?.message || 'Failed to load connected platforms');
        return of([]);
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Get a single platform connection by ID
   * GET /api/social-platforms/{connectionId} (STORE-SCOPED)
   */
  loadPlatform(connectionId: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<SocialPlatform>(`${this.API_URL}/${connectionId}`)
      .pipe(
        tap(platform => {
          // Update in list or add
          this._platforms.update(platforms => {
            const index = platforms.findIndex(p => p.id === connectionId);
            if (index >= 0) {
              const updated = [...platforms];
              updated[index] = platform;
              return updated;
            }
            return [...platforms, platform];
          });
        }),
        catchError(err => {
          this._error.set(err.error?.message || 'Failed to load platform');
          return of(null);
        }),
        finalize(() => this._isLoading.set(false))
      )
      .subscribe();
  }

  /**
   * Connect Facebook platform
   * POST /api/social-platforms/facebook/connect (STORE-SCOPED)
   */
  connectFacebook(request: ConnectFacebookRequest): Observable<SocialPlatform> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.post<SocialPlatform>(`${this.API_URL}/facebook/connect`, request).pipe(
      tap(platform => {
        // Check if platform already exists and update, otherwise add
        this._platforms.update(platforms => {
          const index = platforms.findIndex(p => p.platformName === 'Facebook');
          if (index >= 0) {
            const updated = [...platforms];
            updated[index] = platform;
            return updated;
          }
          return [...platforms, platform];
        });
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to connect Facebook');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Disconnect a platform
   * PUT /api/social-platforms/{connectionId}/disconnect (STORE-SCOPED)
   */
  disconnectPlatform(connectionId: string): Observable<SocialPlatform> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.put<SocialPlatform>(`${this.API_URL}/${connectionId}/disconnect`, {}).pipe(
      tap(platform => {
        // Update platform in list to show disconnected
        this._platforms.update(platforms => platforms.map(p => p.id === connectionId ? platform : p));
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to disconnect platform');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  clearError(): void {
    this._error.set(null);
  }
}

import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap, catchError, finalize } from 'rxjs/operators';
import { Store, CreateStoreRequest } from '../../../core/models/api.models';
import { environment } from '../../../../environments/environment';
import { StoreContextService } from '../../../core/services/store-context.service';

export interface UpdateStoreRequest {
  storeName?: string;
  storeDescription?: string;
  storeAddress?: string;
}

@Injectable({
  providedIn: 'root'
})
export class StoreService {
  private http = inject(HttpClient);
  private storeContext = inject(StoreContextService);

  // State signals
  private readonly _stores = signal<Store[]>([]);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Public computed signals
  readonly stores = computed(() => this._stores());
  readonly isLoading = computed(() => this._isLoading());
  readonly error = computed(() => this._error());
  readonly hasStores = computed(() => this._stores().length > 0);

  // Get current selected store
  readonly currentStore = computed(() => {
    const storeId = this.storeContext.currentStoreId();
    return this._stores().find(s => s.id === storeId) || null;
  });

  /**
   * Load all stores for current user
   * GET /api/stores/my
   */
  loadMyStores(): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<Store[]>(`${environment.apiUrl}/stores/my`).subscribe({
      next: (stores) => {
        this._stores.set(stores);
        this._isLoading.set(false);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to load stores');
        this._isLoading.set(false);
      }
    });
  }

  /**
   * Get a single store by ID
   * GET /api/stores/{storeId}
   */
  loadStore(storeId: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<Store>(`${environment.apiUrl}/stores/${storeId}`).subscribe({
      next: (store) => {
        // Update store in list if exists, otherwise add it
        this._stores.update(stores => {
          const index = stores.findIndex(s => s.id === storeId);
          if (index >= 0) {
            const updated = [...stores];
            updated[index] = store;
            return updated;
          }
          return [...stores, store];
        });
        this.storeContext.updateStoreDetails(store);
        this._isLoading.set(false);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to load store');
        this._isLoading.set(false);
      }
    });
  }

  /**
   * Create a new store
   * POST /api/stores
   */
  createStore(data: CreateStoreRequest): Observable<Store> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.post<Store>(`${environment.apiUrl}/stores`, data).pipe(
      tap(store => {
        this._stores.update(stores => [...stores, store]);
        // Auto-select new store
        this.storeContext.setCurrentStore(store);
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to create store');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Update a store
   * PUT /api/stores/{storeId}
   */
  updateStore(storeId: string, data: UpdateStoreRequest): Observable<Store> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.put<Store>(`${environment.apiUrl}/stores/${storeId}`, data).pipe(
      tap(store => {
        this._stores.update(stores => stores.map(s => s.id === storeId ? store : s));
        this.storeContext.updateStoreDetails(store);
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to update store');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Select a store for the current session
   */
  selectStore(store: Store): void {
    this.storeContext.setCurrentStore(store);
  }

  /**
   * Clear error state
   */
  clearError(): void {
    this._error.set(null);
  }
}

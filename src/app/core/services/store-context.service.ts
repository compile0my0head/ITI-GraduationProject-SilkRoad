import { Injectable, signal, computed, inject } from '@angular/core';
import { Store } from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class StoreContextService {
  private readonly STORE_KEY = 'currentStoreId';

  // Signals
  private _currentStoreId = signal<string | null>(this.loadFromStorage());
  private _currentStore = signal<Store | null>(null);

  // Public readonly signals
  readonly currentStoreId = this._currentStoreId.asReadonly();
  readonly currentStore = this._currentStore.asReadonly();
  readonly hasStore = computed(() => !!this._currentStoreId());

  private loadFromStorage(): string | null {
    if (typeof localStorage !== 'undefined') {
      return localStorage.getItem(this.STORE_KEY);
    }
    return null;
  }

  setCurrentStore(store: Store): void {
    this._currentStoreId.set(store.id);
    this._currentStore.set(store);
    localStorage.setItem(this.STORE_KEY, store.id);
  }

  setCurrentStoreId(storeId: string): void {
    this._currentStoreId.set(storeId);
    localStorage.setItem(this.STORE_KEY, storeId);
  }

  updateStoreDetails(store: Store): void {
    this._currentStore.set(store);
  }

  clearStore(): void {
    this._currentStoreId.set(null);
    this._currentStore.set(null);
    localStorage.removeItem(this.STORE_KEY);
  }

  getStoreIdForHeader(): string | null {
    return this._currentStoreId();
  }
}

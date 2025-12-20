import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { StoreService } from '../services/store.service';
import { AuthService } from '../../../core/auth/services/auth.service';
import { StoreContextService } from '../../../core/services/store-context.service';
import { Store, CreateStoreRequest } from '../../../core/models/api.models';
import { validateMinLength, validateMaxLength } from '../../../shared/utils/validators';

@Component({
  selector: 'app-store-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './store-home.component.html',
  styleUrls: ['./store-home.component.scss']
})
export class StoreHomeComponent implements OnInit {
  private storeService = inject(StoreService);
  private authService = inject(AuthService);
  private storeContext = inject(StoreContextService);
  private router = inject(Router);

  // State
  readonly stores = this.storeService.stores;
  readonly isLoading = this.storeService.isLoading;
  readonly error = this.storeService.error;
  readonly hasStores = this.storeService.hasStores;
  readonly user = this.authService.user;

  // Create store modal
  readonly showCreateModal = signal(false);
  readonly newStoreName = signal('');
  readonly newStoreDescription = signal('');
  readonly storeNameTouched = signal(false);
  readonly createSubmitAttempted = signal(false);

  // Validation computed
  readonly storeNameValidation = computed(() => {
    const value = this.newStoreName();
    if (!value && !this.storeNameTouched() && !this.createSubmitAttempted()) {
      return { valid: true, message: '' };
    }
    if (!value || value.trim() === '') {
      return { valid: false, message: 'Store name is required' };
    }
    const minCheck = validateMinLength(value, 3, 'Store name');
    if (!minCheck.valid) return minCheck;
    return validateMaxLength(value, 50, 'Store name');
  });

  readonly showStoreNameError = computed(() => {
    return (this.storeNameTouched() || this.createSubmitAttempted()) && !this.storeNameValidation().valid;
  });

  readonly isCreateValid = computed(() => {
    return this.storeNameValidation().valid && this.newStoreName().trim().length >= 3;
  });

  ngOnInit(): void {
    // Clear the current store context when entering store selection page
    // This ensures we don't carry stale store context
    this.storeContext.clearStore();
    
    this.storeService.loadMyStores();
  }

  selectStore(store: Store): void {
    this.storeService.selectStore(store);
    this.router.navigate(['/app/dashboard']);
  }

  openCreateModal(): void {
    this.newStoreName.set('');
    this.newStoreDescription.set('');
    this.storeNameTouched.set(false);
    this.createSubmitAttempted.set(false);
    this.showCreateModal.set(true);
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
  }

  onStoreNameBlur(): void {
    this.storeNameTouched.set(true);
  }

  createStore(): void {
    this.createSubmitAttempted.set(true);
    
    if (!this.isCreateValid()) return;

    const data: CreateStoreRequest = {
      storeName: this.newStoreName().trim(),
      storeDescription: this.newStoreDescription().trim() || undefined
    };

    this.storeService.createStore(data).subscribe({
      next: () => {
        this.closeCreateModal();
        // Reload stores list to show the newly created store
        this.storeService.loadMyStores();
      },
      error: (err) => {
        console.error('Failed to create store:', err);
        // Error is already set in the service, modal stays open to show error
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }

  clearError(): void {
    this.storeService.clearError();
  }

  goToLanding(): void {
    localStorage.removeItem('currentStoreId');
    this.storeContext.clearStore();
    this.router.navigate(['/']);
  }
}

import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap, catchError, finalize } from 'rxjs/operators';
import { Customer, CreateCustomerRequest, UpdateCustomerRequest } from '../../../core/models/api.models';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CustomersService {
  private http = inject(HttpClient);

  // State signals
  private readonly _customers = signal<Customer[]>([]);
  private readonly _selectedCustomer = signal<Customer | null>(null);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Public computed signals
  readonly customers = computed(() => this._customers());
  readonly selectedCustomer = computed(() => this._selectedCustomer());
  readonly isLoading = computed(() => this._isLoading());
  readonly error = computed(() => this._error());
  readonly totalCustomers = computed(() => this._customers().length);

  /**
   * Load all customers for current store
   * GET /api/customers
   */
  loadCustomers(): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<Customer[]>(`${environment.apiUrl}/customers`).subscribe({
      next: (customers) => {
        this._customers.set(customers);
        this._isLoading.set(false);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to load customers');
        this._isLoading.set(false);
      }
    });
  }

  /**
   * Get a single customer by ID
   * GET /api/customers/:id
   */
  loadCustomer(id: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<Customer>(`${environment.apiUrl}/customers/${id}`).subscribe({
      next: (customer) => {
        this._selectedCustomer.set(customer);
        this._isLoading.set(false);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to load customer');
        this._isLoading.set(false);
      }
    });
  }

  /**
   * Get customer by ID (returns Observable)
   * GET /api/customers/:id
   */
  getCustomerById(id: string): Observable<Customer> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.get<Customer>(`${environment.apiUrl}/customers/${id}`).pipe(
      tap(customer => {
        this._selectedCustomer.set(customer);
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to load customer');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Create a new customer
   * POST /api/customers
   */
  createCustomer(data: CreateCustomerRequest): Observable<Customer> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.post<Customer>(`${environment.apiUrl}/customers`, data).pipe(
      tap(customer => this._customers.update(customers => [...customers, customer])),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to create customer');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Update an existing customer
   * PUT /api/customers/:id
   */
  updateCustomer(id: string, data: UpdateCustomerRequest): Observable<Customer> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.put<Customer>(`${environment.apiUrl}/customers/${id}`, data).pipe(
      tap(customer => {
        this._customers.update(customers => customers.map(c => c.id === id ? customer : c));
        this._selectedCustomer.set(customer);
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to update customer');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Delete a customer
   * DELETE /api/customers/:id
   */
  deleteCustomer(id: string): Observable<void> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.delete<void>(`${environment.apiUrl}/customers/${id}`).pipe(
      tap(() => this._customers.update(customers => customers.filter(c => c.id !== id))),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to delete customer');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Clear selected customer
   */
  clearSelectedCustomer(): void {
    this._selectedCustomer.set(null);
  }

  /**
   * Clear error state
   */
  clearError(): void {
    this._error.set(null);
  }
}

import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap, catchError, finalize, map } from 'rxjs/operators';
import { Product, CreateProductRequest, UpdateProductRequest } from '../../../core/models/api.models';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProductsService {
  private http = inject(HttpClient);

  // State signals
  private readonly _products = signal<Product[]>([]);
  private readonly _selectedProduct = signal<Product | null>(null);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Public computed signals
  readonly products = computed(() => this._products());
  readonly selectedProduct = computed(() => this._selectedProduct());
  readonly isLoading = computed(() => this._isLoading());
  readonly error = computed(() => this._error());
  readonly totalProducts = computed(() => this._products().length);

  /**
   * Load all products for current store
   * GET /api/products
   * Optional query param: ?inStockOnly=true
   */
  loadProducts(inStockOnly?: boolean): void {
    this._isLoading.set(true);
    this._error.set(null);

    let params = new HttpParams();
    if (inStockOnly) {
      params = params.set('inStockOnly', 'true');
    }

    this.http.get<{ products: Product[] }>(`${environment.apiUrl}/products`, { params }).subscribe({
      next: (res) => {
        this._products.set(res.products); // ✅ FIX
        this._isLoading.set(false);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to load products');
        this._isLoading.set(false);
      }
    });
  }

  /**
   * Get a single product by ID
   * GET /api/products/{productId}
   */
  loadProduct(id: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<Product>(`${environment.apiUrl}/products/${id}`).subscribe({
      next: (product) => {
        this._selectedProduct.set(product);
        this._isLoading.set(false);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to load product');
        this._isLoading.set(false);
      }
    });
  }

  /**
   * Get product by ID (returns Observable)
   * GET /api/products/{productId}
   */
  getProductById(id: string): Observable<Product> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.get<Product>(`${environment.apiUrl}/products/${id}`).pipe(
      tap(product => {
        this._selectedProduct.set(product);
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to load product');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Create a new product
   * POST /api/products
   */
  createProduct(data: CreateProductRequest): Observable<Product> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.post<Product>(`${environment.apiUrl}/products`, data).pipe(
      tap(product => this._products.update(products => [...products, product])),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to create product');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Update an existing product
   * PUT /api/products/{productId}
   */
  updateProduct(id: string, data: UpdateProductRequest): Observable<Product> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.put<Product>(`${environment.apiUrl}/products/${id}`, data).pipe(
      tap(product => {
        this._products.update(products => products.map(p => p.id === id ? product : p));
        this._selectedProduct.set(product);
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to update product');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Delete a product
   * DELETE /api/products/{productId}
   */
  deleteProduct(id: string): Observable<void> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.delete<void>(`${environment.apiUrl}/products/${id}`).pipe(
      tap(() => this._products.update(products => products.filter(p => p.id !== id))),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to delete product');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Clear selected product
   */
  clearSelectedProduct(): void {
    this._selectedProduct.set(null);
  }

  /**
   * Clear error state
   */
  clearError(): void {
    this._error.set(null);
  }
}

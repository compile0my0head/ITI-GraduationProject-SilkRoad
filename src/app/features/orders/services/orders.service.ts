import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Order, OrderStatus, CreateOrderRequest, OrderProduct } from '../../../core/models/api.models';
import { environment } from '../../../../environments/environment';
import { Observable, of } from 'rxjs';
import { timeout, tap, catchError, finalize, map } from 'rxjs/operators';

export interface OrderFilters {
  status?: OrderStatus;
  customerId?: string;
  fromDate?: string;
  toDate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class OrdersService {
  private http = inject(HttpClient);

  // State signals
  private readonly _orders = signal<Order[]>([]);
  private readonly _selectedOrder = signal<Order | null>(null);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Public computed signals
  readonly orders = computed(() => this._orders());
  readonly selectedOrder = computed(() => this._selectedOrder());
  readonly isLoading = computed(() => this._isLoading());
  readonly error = computed(() => this._error());
  readonly totalOrders = computed(() => this._orders().length);

  // Computed - orders by status (using string literals)
  readonly pendingOrders = computed(() => 
    this._orders().filter(o => o.status === 'Pending')
  );
  readonly acceptedOrders = computed(() => 
    this._orders().filter(o => o.status === 'Accepted')
  );
  readonly shippedOrders = computed(() => 
    this._orders().filter(o => o.status === 'Shipped')
  );
  readonly deliveredOrders = computed(() => 
    this._orders().filter(o => o.status === 'Delivered')
  );
  readonly pendingCount = computed(() => this.pendingOrders().length);

  /**
   * Load all orders for current store
   * GET /api/orders
   * Optional query param: ?status={status}
   */
  loadOrders(filters?: OrderFilters): void {
    this._isLoading.set(true);
    this._error.set(null);

    let params = new HttpParams();
    if (filters?.status) params = params.set('status', filters.status);

    this.http.get<Order[]>(`${environment.apiUrl}/orders`, { params })
      .pipe(timeout(15000))
      .subscribe({
      next: (orders) => {
        this._orders.set(orders);
        this._isLoading.set(false);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to load orders');
        this._isLoading.set(false);
      }
    });
  }

  /**
   * Get a single order by ID
   * GET /api/orders/{orderId}
   */
  loadOrder(id: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<Order>(`${environment.apiUrl}/orders/${id}`)
      .pipe(timeout(15000))
      .subscribe({
      next: (order) => {
        this._selectedOrder.set(order);
        this._isLoading.set(false);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to load order');
        this._isLoading.set(false);
      }
    });
  }

  /**
   * Get a single order by ID (returns Observable for direct subscription)
   * GET /api/orders/{orderId}
   */
  getOrderById(id: string): Observable<Order> {
    return this.http.get<Order>(`${environment.apiUrl}/orders/${id}`).pipe(
      timeout(15000),
      tap(order => this._selectedOrder.set(order)),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to load order');
        throw err;
      })
    );
  }

  /**
   * Get orders by customer ID
   * GET /api/orders/by-customer/{customerId}
   */
  loadOrdersByCustomer(customerId: string): Observable<Order[]> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.get<Order[]>(`${environment.apiUrl}/orders/by-customer/${customerId}`).pipe(
      timeout(15000),
      tap(orders => {
        this._orders.set(orders);
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to load customer orders');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Create a new order
   * POST /api/orders
   */
  createOrder(data: CreateOrderRequest): Observable<Order> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.post<Order>(`${environment.apiUrl}/orders`, data).pipe(
      timeout(15000),
      tap(order => this._orders.update(orders => [order, ...orders])),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to create order');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Update order (general update)
   * PUT /api/orders/{orderId}
   */
  updateOrder(id: string, data: { status: OrderStatus }): Observable<Order> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.put<Order>(`${environment.apiUrl}/orders/${id}`, data).pipe(
      timeout(15000),
      tap(order => {
        this._orders.update(orders => orders.map(o => o.id === id ? order : o));
        if (this._selectedOrder()?.id === id) {
          this._selectedOrder.set(order);
        }
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to update order');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Accept a pending order
   * PUT /api/orders/{orderId}/accept
   */
  acceptOrder(id: string): Observable<Order> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.put<Order>(`${environment.apiUrl}/orders/${id}/accept`, {}).pipe(
      timeout(15000),
      tap(order => {
        this._orders.update(orders => orders.map(o => o.id === id ? order : o));
        if (this._selectedOrder()?.id === id) {
          this._selectedOrder.set(order);
        }
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to accept order');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Reject a pending order
   * PUT /api/orders/{orderId}/reject
   */
  rejectOrder(id: string): Observable<Order> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.put<Order>(`${environment.apiUrl}/orders/${id}/reject`, {}).pipe(
      timeout(15000),
      tap(order => {
        this._orders.update(orders => orders.map(o => o.id === id ? order : o));
        if (this._selectedOrder()?.id === id) {
          this._selectedOrder.set(order);
        }
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to reject order');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Load products for a specific order
   * GET /api/OrderProduct/order/{orderId}
   */
  loadOrderProducts(orderId: string): Observable<OrderProduct[]> {
    return this.http.get<OrderProduct[]>(`${environment.apiUrl}/OrderProduct/order/${orderId}`).pipe(
      timeout(15000),
      catchError(err => {
        console.error('Failed to load order products:', err);
        return of([]);
      })
    );
  }

  /**
   * Clear selected order
   */
  clearSelectedOrder(): void {
    this._selectedOrder.set(null);
  }

  /**
   * Clear error state
   */
  clearError(): void {
    this._error.set(null);
  }
}

import { Component, inject, signal, computed, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { StoreService } from '../stores/services/store.service';
import { Order, Product, Customer, Campaign } from '../../core/models/api.models';
import { environment } from '../../../environments/environment';

interface DashboardStats {
  totalProducts: number;
  totalCustomers: number;
  totalOrders: number;
  pendingOrders: number;
  activeCampaigns: number;
  revenue: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private storeService = inject(StoreService);
  private pollingInterval: any;

  // State
  readonly currentStore = this.storeService.currentStore;
  readonly stats = signal<DashboardStats>({
    totalProducts: 0,
    totalCustomers: 0,
    totalOrders: 0,
    pendingOrders: 0,
    activeCampaigns: 0,
    revenue: 0
  });
  readonly pendingOrders = signal<Order[]>([]);
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);

  // Computed
  readonly hasPendingOrders = computed(() => this.pendingOrders().length > 0);

  ngOnInit(): void {
    this.loadDashboardData();
    // Poll for pending orders every 30 seconds (as per documentation)
    this.pollingInterval = setInterval(() => {
      this.loadPendingOrders();
    }, 30000);
  }

  ngOnDestroy(): void {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }
  }

  loadDashboardData(): void {
    this.isLoading.set(true);
    this.error.set(null);

    // Fetch all data in parallel using forkJoin
    forkJoin({
      products: this.http.get<Product[]>(`${environment.apiUrl}/products`),
      customers: this.http.get<Customer[]>(`${environment.apiUrl}/customers`),
      orders: this.http.get<Order[]>(`${environment.apiUrl}/orders`),
      pendingOrders: this.http.get<Order[]>(`${environment.apiUrl}/orders?status=Pending`),
      campaigns: this.http.get<Campaign[]>(`${environment.apiUrl}/campaigns`)
    }).subscribe({
      next: (data) => {
        // Calculate revenue from delivered orders
        const revenue = data.orders
          .filter(o => o.status === 'Delivered')
          .reduce((sum, o) => sum + o.totalPrice, 0);

        // Count active campaigns (Published or Scheduled)
        const activeCampaigns = data.campaigns
          .filter(c => c.campaignStage === 'Published' || c.campaignStage === 'Scheduled')
          .length;

        this.stats.set({
          totalProducts: data.products.length,
          totalCustomers: data.customers.length,
          totalOrders: data.orders.length,
          pendingOrders: data.pendingOrders.length,
          activeCampaigns: activeCampaigns,
          revenue: revenue
        });

        this.pendingOrders.set(data.pendingOrders);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load dashboard data:', err);
        this.error.set('Failed to load dashboard data. Please try again.');
        this.isLoading.set(false);
      }
    });
  }

  loadPendingOrders(): void {
    // Silent refresh for polling - don't show loading state
    this.http.get<Order[]>(`${environment.apiUrl}/orders?status=Pending`).subscribe({
      next: (orders) => {
        this.pendingOrders.set(orders);
        this.stats.update(s => ({ ...s, pendingOrders: orders.length }));
      },
      error: (err) => {
        console.error('Failed to refresh pending orders:', err);
      }
    });
  }

  acceptOrder(order: Order): void {
    this.http.put<Order>(`${environment.apiUrl}/orders/${order.id}/accept`, {}).subscribe({
      next: () => {
        this.pendingOrders.update(orders => orders.filter(o => o.id !== order.id));
        this.stats.update(s => ({ ...s, pendingOrders: s.pendingOrders - 1 }));
      },
      error: (err) => {
        console.error('Failed to accept order:', err);
        this.error.set('Failed to accept order. Please try again.');
      }
    });
  }

  rejectOrder(order: Order): void {
    if (confirm('Are you sure you want to reject this order?')) {
      this.http.put<Order>(`${environment.apiUrl}/orders/${order.id}/reject`, {}).subscribe({
        next: () => {
          this.pendingOrders.update(orders => orders.filter(o => o.id !== order.id));
          this.stats.update(s => ({ ...s, pendingOrders: s.pendingOrders - 1 }));
        },
        error: (err) => {
          console.error('Failed to reject order:', err);
          this.error.set('Failed to reject order. Please try again.');
        }
      });
    }
  }

  clearError(): void {
    this.error.set(null);
  }
}

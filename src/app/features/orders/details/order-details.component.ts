import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { OrdersService } from '../services/orders.service';
import { Order, OrderStatus, OrderProduct } from '../../../core/models/api.models';

@Component({
  selector: 'app-order-details',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-details.component.html',
  styleUrls: ['./order-details.component.scss']
})
export class OrderDetailsComponent implements OnInit {
  private ordersService = inject(OrdersService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // State
  order = signal<Order | null>(null);
  orderProducts = signal<OrderProduct[]>([]);
  isLoading = signal(true);
  isLoadingProducts = signal(false);
  isUpdating = signal(false);
  error = signal<string | null>(null);

  // Computed
  canUpdateStatus = computed(() => {
    const order = this.order();
    return order && order.status === 'Pending';
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadOrder(id);
    } else {
      this.error.set('No order ID provided');
      this.isLoading.set(false);
    }
  }

  private loadOrder(id: string): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.ordersService.getOrderById(id).subscribe({
      next: (order) => {
        this.order.set(order);
        this.isLoading.set(false);
        // Always load order products from OrderProduct API
        this.loadOrderProducts(order.id);
      },
      error: (err) => {
        console.error('Failed to load order:', err);
        this.error.set(err.error?.message || 'Failed to load order details');
        this.isLoading.set(false);
      }
    });
  }

  private loadOrderProducts(orderId: string): void {
    this.isLoadingProducts.set(true);
    this.ordersService.loadOrderProducts(orderId).subscribe({
      next: (products) => {
        this.orderProducts.set(products);
        this.isLoadingProducts.set(false);
      },
      error: (err) => {
        console.error('Failed to load order products:', err);
        // Don't set error - products might just not exist or use order.items instead
        this.isLoadingProducts.set(false);
      }
    });
  }

  acceptOrder(): void {
    const order = this.order();
    if (!order || this.isUpdating()) return;

    this.isUpdating.set(true);
    this.ordersService.acceptOrder(order.id).subscribe({
      next: (updated) => {
        this.order.set(updated);
        this.isUpdating.set(false);
      },
      error: (err: any) => {
        this.error.set(err.error?.message || 'Failed to accept order');
        this.isUpdating.set(false);
      }
    });
  }

  rejectOrder(): void {
    const order = this.order();
    if (!order || this.isUpdating()) return;

    if (confirm('Are you sure you want to reject this order?')) {
      this.isUpdating.set(true);
      this.ordersService.rejectOrder(order.id).subscribe({
        next: (updated) => {
          this.order.set(updated);
          this.isUpdating.set(false);
        },
        error: (err: any) => {
          this.error.set(err.error?.message || 'Failed to reject order');
          this.isUpdating.set(false);
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/app/orders']);
  }

  formatDate(date: string | Date): string {
    return new Date(date).toLocaleDateString('en-US', {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  getStatusClass(status: OrderStatus): string {
    switch (status) {
      case 'Pending': return 'status-pending';
      case 'Accepted': return 'status-accepted';
      case 'Rejected': return 'status-rejected';
      case 'Delivered': return 'status-delivered';
      case 'Cancelled': return 'status-cancelled';
      default: return '';
    }
  }

  getStatusIcon(status: OrderStatus): string {
    switch (status) {
      case 'Pending': return '⏳';
      case 'Accepted': return '✅';
      case 'Rejected': return '❌';
      case 'Delivered': return '📦';
      case 'Cancelled': return '🚫';
      default: return '📋';
    }
  }
}

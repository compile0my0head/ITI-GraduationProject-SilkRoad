import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { OrdersService } from '../services/orders.service';
import { Order, OrderStatus } from '../../../core/models/api.models';

// Define order status values for template usage
const ORDER_STATUSES: { value: OrderStatus | '', label: string }[] = [
  { value: '', label: 'All Orders' },
  { value: 'Pending', label: 'Pending' },
  { value: 'Accepted', label: 'Accepted' },
  { value: 'Shipped', label: 'Shipped' },
  { value: 'Delivered', label: 'Delivered' },
  { value: 'Rejected', label: 'Rejected' },
  { value: 'Cancelled', label: 'Cancelled' }
];

@Component({
  selector: 'app-orders-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './orders-list.component.html',
  styleUrls: ['./orders-list.component.scss']
})
export class OrdersListComponent implements OnInit {
  private ordersService = inject(OrdersService);
  private route = inject(ActivatedRoute);

  // State from service
  readonly orders = this.ordersService.orders;
  readonly isLoading = this.ordersService.isLoading;
  readonly error = this.ordersService.error;

  // Local state
  readonly statusFilter = signal<OrderStatus | ''>('');
  readonly searchQuery = signal('');

  // All status options for filter
  readonly statusOptions = ORDER_STATUSES;

  // Computed - filtered orders
  readonly filteredOrders = computed(() => {
    let result = this.orders();
    
    const status = this.statusFilter();
    if (status) {
      result = result.filter(o => o.status === status);
    }

    const query = this.searchQuery().toLowerCase();
    if (query) {
      result = result.filter(o => 
        o.id.toLowerCase().includes(query) ||
        o.customerName?.toLowerCase().includes(query) ||
        o.customer?.customerName?.toLowerCase().includes(query) ||
        o.customer?.phone?.toLowerCase().includes(query)
      );
    }

    return result;
  });

  readonly hasOrders = computed(() => this.orders().length > 0);

  ngOnInit(): void {
    // Check for status query param
    const statusParam = this.route.snapshot.queryParams['status'] as OrderStatus;
    if (statusParam && ORDER_STATUSES.some(s => s.value === statusParam)) {
      this.statusFilter.set(statusParam);
    }
    this.ordersService.loadOrders();
  }

  acceptOrder(order: Order): void {
    this.ordersService.acceptOrder(order.id).subscribe({
      error: () => {
        // Error handled by service
      }
    });
  }

  rejectOrder(order: Order): void {
    this.ordersService.rejectOrder(order.id).subscribe({
      error: () => {
        // Error handled by service
      }
    });
  }

  getStatusClass(status: OrderStatus): string {
    const classes: Record<OrderStatus, string> = {
      'Pending': 'status-pending',
      'Accepted': 'status-accepted',
      'Shipped': 'status-shipped',
      'Delivered': 'status-delivered',
      'Rejected': 'status-rejected',
      'Cancelled': 'status-cancelled',
      'Refunded': 'status-refunded'
    };
    return classes[status] || '';
  }

  clearError(): void {
    this.ordersService.clearError();
  }
}

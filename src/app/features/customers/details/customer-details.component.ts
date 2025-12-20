import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CustomersService } from '../services/customers.service';
import { OrdersService } from '../../orders/services/orders.service';
import { Customer, Order } from '../../../core/models/api.models';

@Component({
  selector: 'app-customer-details',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './customer-details.component.html',
  styleUrls: ['./customer-details.component.scss']
})
export class CustomerDetailsComponent implements OnInit {
  private customersService = inject(CustomersService);
  private ordersService = inject(OrdersService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // State
  readonly customer = signal<Customer | null>(null);
  readonly isLoading = signal(false);
  readonly isLoadingOrders = signal(false);
  readonly error = signal<string | null>(null);
  readonly customerOrders = signal<Order[]>([]);

  // Computed
  readonly hasOrders = computed(() => this.customerOrders().length > 0);
  readonly totalOrderValue = computed(() => 
    this.customerOrders().reduce((sum, order) => sum + order.totalPrice, 0)
  );

  ngOnInit(): void {
    const customerId = this.route.snapshot.paramMap.get('id');
    if (customerId) {
      this.loadCustomerData(customerId);
    } else {
      this.error.set('No customer ID provided');
    }
  }

  private loadCustomerData(customerId: string): void {
    this.isLoading.set(true);
    this.error.set(null);
    
    // Load customer details
    this.customersService.getCustomerById(customerId).subscribe({
      next: (customer) => {
        this.customer.set(customer);
        this.isLoading.set(false);
        // Load customer orders
        this.loadCustomerOrders(customerId);
      },
      error: (err) => {
        console.error('Failed to load customer:', err);
        this.error.set(err.error?.message || 'Failed to load customer details');
        this.isLoading.set(false);
      }
    });
  }

  private loadCustomerOrders(customerId: string): void {
    this.isLoadingOrders.set(true);
    this.ordersService.loadOrdersByCustomer(customerId).subscribe({
      next: (orders) => {
        this.customerOrders.set(orders);
        this.isLoadingOrders.set(false);
      },
      error: (err) => {
        console.error('Failed to load customer orders:', err);
        this.isLoadingOrders.set(false);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/app/customers']);
  }

  formatDate(date: string | Date): string {
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  getStatusClass(status: string): string {
    return `status-${status.toLowerCase()}`;
  }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'Pending': return '⏳';
      case 'Accepted': return '✅';
      case 'Rejected': return '❌';
      case 'Delivered': return '📦';
      case 'Cancelled': return '🚫';
      case 'Shipped': return '🚚';
      case 'Refunded': return '💰';
      default: return '📋';
    }
  }

  navigateToOrder(orderId: string): void {
    this.router.navigate(['/app/orders', orderId]);
  }

  clearError(): void {
    this.error.set(null);
    this.customersService.clearError();
  }
}

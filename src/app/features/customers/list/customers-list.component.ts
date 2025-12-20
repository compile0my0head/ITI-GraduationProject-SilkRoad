import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CustomersService } from '../services/customers.service';
import { Customer } from '../../../core/models/api.models';

@Component({
  selector: 'app-customers-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './customers-list.component.html',
  styleUrls: ['./customers-list.component.scss']
})
export class CustomersListComponent implements OnInit {
  private customersService = inject(CustomersService);

  // State from service
  readonly customers = this.customersService.customers;
  readonly isLoading = this.customersService.isLoading;
  readonly error = this.customersService.error;

  // Local state
  readonly searchQuery = signal('');
  readonly showDeleteModal = signal(false);
  readonly customerToDelete = signal<Customer | null>(null);

  // Computed - filtered customers (using correct property names)
  readonly filteredCustomers = computed(() => {
    const query = this.searchQuery().toLowerCase();
    if (!query) return this.customers();
    return this.customers().filter(c => 
      c.customerName.toLowerCase().includes(query) ||
      c.phone?.toLowerCase().includes(query) ||
      c.billingAddress?.toLowerCase().includes(query)
    );
  });

  readonly hasCustomers = computed(() => this.customers().length > 0);

  ngOnInit(): void {
    this.customersService.loadCustomers();
  }

  openDeleteModal(customer: Customer): void {
    this.customerToDelete.set(customer);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.customerToDelete.set(null);
  }

  confirmDelete(): void {
    const customer = this.customerToDelete();
    if (!customer) return;

    this.customersService.deleteCustomer(customer.id).subscribe({
      next: () => this.closeDeleteModal(),
      error: () => {
        // Error is handled by service
      }
    });
  }

  clearError(): void {
    this.customersService.clearError();
  }
}

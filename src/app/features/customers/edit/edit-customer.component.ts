import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CustomersService } from '../services/customers.service';
import { validateEmail, validateRequired, validatePhone, validateFullName } from '../../../shared/utils/validators';
import { Customer } from '../../../core/models/api.models';

@Component({
  selector: 'app-edit-customer',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './edit-customer.component.html',
  styleUrls: ['./edit-customer.component.scss']
})
export class EditCustomerComponent implements OnInit {
  private customersService = inject(CustomersService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // Customer ID
  customerId = signal<string | null>(null);

  // Form Data
  name = signal('');
  phone = signal('');
  address = signal('');

  // Touched states
  nameTouched = signal(false);
  phoneTouched = signal(false);
  formSubmitAttempted = signal(false);

  // UI State
  isLoading = signal(true);
  isSubmitting = signal(false);
  error = signal<string | null>(null);

  // Validation computed signals
  nameValidation = computed(() => {
    const value = this.name();
    if (!value && !this.nameTouched() && !this.formSubmitAttempted()) {
      return { valid: true, message: '' };
    }
    return validateFullName(value);
  });

  phoneValidation = computed(() => {
    const value = this.phone();
    if (!value) {
      return { valid: true, message: '' };
    }
    return validatePhone(value);
  });

  // Show error computed
  showNameError = computed(() => {
    return (this.nameTouched() || this.formSubmitAttempted()) && !this.nameValidation().valid;
  });

  showPhoneError = computed(() => {
    return this.phoneTouched() && !this.phoneValidation().valid;
  });

  // Form valid computed
  isFormValid = computed(() => {
    return this.nameValidation().valid && 
           this.phoneValidation().valid &&
           this.name().trim().length > 0;
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.customerId.set(id);
      this.loadCustomer(id);
    } else {
      this.error.set('No customer ID provided');
      this.isLoading.set(false);
    }
  }

  private loadCustomer(id: string): void {
    this.isLoading.set(true);
    this.customersService.getCustomerById(id).subscribe({
      next: (customer) => {
        this.populateForm(customer);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load customer:', err);
        this.error.set(err.error?.message || 'Failed to load customer');
        this.isLoading.set(false);
      }
    });
  }

  private populateForm(customer: Customer): void {
    this.name.set(customer.customerName || '');
    this.phone.set(customer.phone || '');
    this.address.set(customer.billingAddress || '');
  }

  // Blur handlers
  onNameBlur(): void {
    this.nameTouched.set(true);
  }

  onPhoneBlur(): void {
    this.phoneTouched.set(true);
  }

  submitForm(): void {
    this.formSubmitAttempted.set(true);
    
    const id = this.customerId();
    if (!this.isFormValid() || this.isSubmitting() || !id) return;

    this.isSubmitting.set(true);
    this.error.set(null);

    this.customersService.updateCustomer(id, {
      customerName: this.name(),
      phone: this.phone() || undefined,
      billingAddress: this.address() || undefined
    }).subscribe({
      next: () => {
        this.router.navigate(['/app/customers']);
      },
      error: (err: any) => {
        this.error.set(err.message || 'Failed to update customer');
        this.isSubmitting.set(false);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/app/customers']);
  }
}

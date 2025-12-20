import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CustomersService } from '../services/customers.service';
import { validateEmail, validateRequired, validatePhone, validateFullName } from '../../../shared/utils/validators';

@Component({
  selector: 'app-add-customer',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './add-customer.component.html',
  styleUrls: ['./add-customer.component.scss']
})
export class AddCustomerComponent {
  private customersService = inject(CustomersService);
  private router = inject(Router);

  // Form Data
  name = signal('');
  email = signal('');
  phone = signal('');
  address = signal('');
  city = signal('');
  country = signal('');
  notes = signal('');

  // Touched states
  nameTouched = signal(false);
  emailTouched = signal(false);
  phoneTouched = signal(false);
  formSubmitAttempted = signal(false);

  // UI State
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

  emailValidation = computed(() => {
    const value = this.email();
    if (!value && !this.emailTouched() && !this.formSubmitAttempted()) {
      return { valid: true, message: '' };
    }
    return validateEmail(value);
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

  showEmailError = computed(() => {
    return (this.emailTouched() || this.formSubmitAttempted()) && !this.emailValidation().valid;
  });

  showPhoneError = computed(() => {
    return this.phoneTouched() && !this.phoneValidation().valid;
  });

  // Form valid computed
  isFormValid = computed(() => {
    return this.nameValidation().valid && 
           this.emailValidation().valid && 
           this.phoneValidation().valid &&
           this.name().trim().length > 0 &&
           this.email().trim().length > 0;
  });

  // Blur handlers
  onNameBlur(): void {
    this.nameTouched.set(true);
  }

  onEmailBlur(): void {
    this.emailTouched.set(true);
  }

  onPhoneBlur(): void {
    this.phoneTouched.set(true);
  }

  submitForm(): void {
    this.formSubmitAttempted.set(true);
    
    if (!this.isFormValid() || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.error.set(null);

    this.customersService.createCustomer({
      customerName: this.name(),
      phone: this.phone() || undefined,
      billingAddress: this.address() || undefined
    }).subscribe({
      next: () => {
        this.router.navigate(['/app/customers']);
      },
      error: (err: any) => {
        this.error.set(err.message || 'Failed to create customer');
        this.isSubmitting.set(false);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/app/customers']);
  }
}

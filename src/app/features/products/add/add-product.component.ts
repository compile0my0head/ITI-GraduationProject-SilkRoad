import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductsService } from '../services/products.service';
import { CreateProductRequest } from '../../../core/models/api.models';
import { 
  validateRequired, 
  validateMinLength, 
  validatePrice, 
  validateStockQuantity,
  validateUrl 
} from '../../../shared/utils/validators';

@Component({
  selector: 'app-add-product',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './add-product.component.html',
  styleUrls: ['./add-product.component.scss']
})
export class AddProductComponent {
  private productsService = inject(ProductsService);
  private router = inject(Router);

  // State from service
  readonly isLoading = this.productsService.isLoading;
  readonly error = this.productsService.error;
  readonly success = signal<string | null>(null);

  // Form fields
  readonly name = signal('');
  readonly description = signal('');
  readonly price = signal<number | null>(null);
  readonly stockQuantity = signal<number | null>(null);
  readonly category = signal('');
  readonly imageUrl = signal('');

  // Track touched state
  readonly nameTouched = signal(false);
  readonly priceTouched = signal(false);
  readonly stockQuantityTouched = signal(false);
  readonly imageUrlTouched = signal(false);
  readonly formSubmitAttempted = signal(false);

  // Validation states
  readonly nameValidation = computed(() => {
    const value = this.name();
    if (!value && !this.nameTouched() && !this.formSubmitAttempted()) {
      return { valid: true, message: '' };
    }
    if (!value || value.trim() === '') {
      return { valid: false, message: 'Product name is required' };
    }
    return validateMinLength(value, 2, 'Product name');
  });

  readonly priceValidation = computed(() => {
    const value = this.price();
    if (value === null && !this.priceTouched() && !this.formSubmitAttempted()) {
      return { valid: true, message: '' };
    }
    return validatePrice(value);
  });

  readonly stockQuantityValidation = computed(() => {
    const value = this.stockQuantity();
    if (value === null && !this.stockQuantityTouched() && !this.formSubmitAttempted()) {
      return { valid: true, message: '' };
    }
    return validateStockQuantity(value);
  });

  readonly imageUrlValidation = computed(() => {
    const value = this.imageUrl();
    if (!value) {
      return { valid: true, message: '' };
    }
    return validateUrl(value);
  });

  // Show error states
  readonly showNameError = computed(() => {
    return (this.nameTouched() || this.formSubmitAttempted()) && !this.nameValidation().valid;
  });

  readonly showPriceError = computed(() => {
    return (this.priceTouched() || this.formSubmitAttempted()) && !this.priceValidation().valid;
  });

  readonly showStockQuantityError = computed(() => {
    return (this.stockQuantityTouched() || this.formSubmitAttempted()) && !this.stockQuantityValidation().valid;
  });

  readonly showImageUrlError = computed(() => {
    return this.imageUrlTouched() && !this.imageUrlValidation().valid;
  });

  // Computed validation
  readonly isFormValid = computed(() => {
    return this.nameValidation().valid &&
           this.priceValidation().valid &&
           this.stockQuantityValidation().valid &&
           this.imageUrlValidation().valid &&
           this.name().trim().length >= 2 &&
           this.price() !== null &&
           this.stockQuantity() !== null;
  });

  // Blur handlers
  onNameBlur(): void {
    this.nameTouched.set(true);
  }

  onPriceBlur(): void {
    this.priceTouched.set(true);
  }

  onStockQuantityBlur(): void {
    this.stockQuantityTouched.set(true);
  }

  onImageUrlBlur(): void {
    this.imageUrlTouched.set(true);
  }

  onSubmit(): void {
    this.formSubmitAttempted.set(true);
    
    if (!this.isFormValid()) return;

    const data: CreateProductRequest = {
      productName: this.name().trim(),
      productDescription: this.description().trim() || undefined,
      productPrice: this.price()!,
      inStock: (this.stockQuantity() || 0) > 0,
      imageUrl: this.imageUrl().trim() || undefined,
      brand: this.category().trim() || undefined
    };

    this.productsService.createProduct(data).subscribe({
      next: () => {
        this.success.set('Product created successfully!');
        // Wait a moment to show success message, then redirect
        setTimeout(() => {
          this.router.navigate(['/app/products']);
        }, 1500);
      },
      error: () => {
        // Error is handled by service
      }
    });
  }

  clearError(): void {
    this.productsService.clearError();
  }
}

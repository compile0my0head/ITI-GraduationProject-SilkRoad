import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductsService } from '../services/products.service';
import { UpdateProductRequest } from '../../../core/models/api.models';
import { validateMinLength, validatePositiveNumber } from '../../../shared/utils/validators';

@Component({
  selector: 'app-edit-product',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './edit-product.component.html',
  styleUrls: ['./edit-product.component.scss']
})
export class EditProductComponent implements OnInit {
  private productsService = inject(ProductsService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // State from service
  readonly selectedProduct = this.productsService.selectedProduct;
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

  // Form touched states
  readonly nameTouched = signal(false);
  readonly priceTouched = signal(false);
  readonly stockQuantityTouched = signal(false);
  readonly submitAttempted = signal(false);

  // Validation computed
  nameValidation = computed(() => {
    const value = this.name();
    if (!value && !this.nameTouched() && !this.submitAttempted()) {
      return { valid: true, message: '' };
    }
    if (!value || value.trim() === '') {
      return { valid: false, message: 'Product name is required' };
    }
    return validateMinLength(value, 2, 'Product name');
  });

  priceValidation = computed(() => {
    const value = this.price();
    if (value === null && !this.priceTouched() && !this.submitAttempted()) {
      return { valid: true, message: '' };
    }
    return validatePositiveNumber(value, 'Price', true);
  });

  stockQuantityValidation = computed(() => {
    const value = this.stockQuantity();
    if (value === null && !this.stockQuantityTouched() && !this.submitAttempted()) {
      return { valid: true, message: '' };
    }
    return validatePositiveNumber(value, 'Stock quantity', true);
  });

  // Show error states
  showNameError = computed(() => {
    return (this.nameTouched() || this.submitAttempted()) && !this.nameValidation().valid;
  });

  showPriceError = computed(() => {
    return (this.priceTouched() || this.submitAttempted()) && !this.priceValidation().valid;
  });

  showStockQuantityError = computed(() => {
    return (this.stockQuantityTouched() || this.submitAttempted()) && !this.stockQuantityValidation().valid;
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

  // Product ID from route
  private productId = '';

  ngOnInit(): void {
    this.productId = this.route.snapshot.params['id'];
    if (this.productId) {
      this.productsService.getProductById(this.productId).subscribe({
        next: (product) => {
          // Populate form fields with fetched product data
          this.name.set(product.productName);
          this.description.set(product.productDescription || '');
          this.price.set(product.productPrice);
          this.stockQuantity.set(product.inStock ? 1 : 0);
          this.category.set(product.brand || '');
          this.imageUrl.set(product.imageUrl || '');
        },
        error: () => {
          // Error is handled by service
        }
      });
    }
  }

  // Computed validation
  readonly isFormValid = computed(() => {
    return this.nameValidation().valid &&
           this.priceValidation().valid &&
           this.stockQuantityValidation().valid &&
           this.name().trim().length >= 2;
  });

  onSubmit(): void {
    this.submitAttempted.set(true);
    
    if (!this.isFormValid()) return;

    const data: UpdateProductRequest = {
      productName: this.name().trim(),
      productDescription: this.description().trim() || undefined,
      productPrice: this.price()!,
      inStock: (this.stockQuantity() || 0) > 0,
      imageUrl: this.imageUrl().trim() || undefined,
      brand: this.category().trim() || undefined
    };

    this.productsService.updateProduct(this.productId, data).subscribe({
      next: () => {
        this.success.set('Product updated successfully!');
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

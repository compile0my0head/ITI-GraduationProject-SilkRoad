import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductsService } from '../services/products.service';
import { Product } from '../../../core/models/api.models';

@Component({
  selector: 'app-products-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './products-list.component.html',
  styleUrls: ['./products-list.component.scss']
})
export class ProductsListComponent implements OnInit {
  private productsService = inject(ProductsService);

  // State from service
  readonly products = this.productsService.products;
  readonly isLoading = this.productsService.isLoading;
  readonly error = this.productsService.error;

  // Local state
  readonly searchQuery = signal('');
  readonly showDeleteModal = signal(false);
  readonly productToDelete = signal<Product | null>(null);

  // Computed - filtered products (using correct property names)
  readonly filteredProducts = computed(() => {
    const query = this.searchQuery().toLowerCase();
    if (!query) return this.products();
    return this.products().filter(p => 
      p.productName.toLowerCase().includes(query) ||
      p.brand?.toLowerCase().includes(query) ||
      p.productDescription?.toLowerCase().includes(query)
    );
  });

  readonly hasProducts = computed(() => this.products().length > 0);

  ngOnInit(): void {
    this.productsService.loadProducts();
  }

  openDeleteModal(product: Product): void {
    this.productToDelete.set(product);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.productToDelete.set(null);
  }

  confirmDelete(): void {
    const product = this.productToDelete();
    if (!product) return;

    this.productsService.deleteProduct(product.id).subscribe({
      next: () => this.closeDeleteModal(),
      error: () => {
        // Error is handled by service
      }
    });
  }

  clearError(): void {
    this.productsService.clearError();
  }
}

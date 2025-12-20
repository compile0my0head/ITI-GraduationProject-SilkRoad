export interface Product {
  id: string;
  externalProductID: string | null;
  storeId: string;
  productName: string;
  productDescription: string;
  productPrice: number;
  inStock: boolean;
  brand: string | null;
  imageUrl: string | null;
  condition: string | null;
  url: string | null;
  retailerId: string | null;
  createdAt: Date; // IMPORTANT
}


export interface CreateProductRequest {
  productName: string;
  productDescription: string;
  productPrice: number;
  inStock: boolean;
}

export interface UpdateProductRequest {
  productName?: string;
  productDescription?: string;
  productPrice?: number;
  inStock?: boolean;
}

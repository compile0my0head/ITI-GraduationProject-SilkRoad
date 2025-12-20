// ============================================
// SilkRoad - Core API Models
// ============================================

// User & Auth
export interface User {
  id: string;
  email: string;
  fullName: string;
  createdAt?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

// Auth response matches backend: { id, email, fullName, token, tokenExpiration }
export interface AuthResponse {
  id: string;
  email: string;
  fullName: string;
  token: string;
  tokenExpiration: string;
}

// Store
export interface Store {
  id: string;
  storeName: string;
  storeDescription?: string;
  storeAddress?: string;
  ownerUserId: string;
  createdAt: string;
}

export interface CreateStoreRequest {
  storeName: string;
  storeDescription?: string;
  storeAddress?: string;
}

// Team (Placeholder)
export interface Team {
  id: string;
  teamName: string;
  description?: string;
  storeId: string;
  storeName: string;
  memberCount: number;
  createdAt: string;
}

// Product
export interface Product {
  id: string;
  productName: string;
  productDescription?: string;
  productPrice: number;
  inStock: boolean;
  imageUrl?: string;
  brand?: string;
  condition?: string;
  storeId: string;
  createdAt: string;
}

export interface CreateProductRequest {
  productName: string;
  productDescription?: string;
  productPrice: number;
  inStock: boolean;
  imageUrl?: string;
  brand?: string;
  condition?: string;
}

export interface UpdateProductRequest extends Partial<CreateProductRequest> {}

// Customer
export interface Customer {
  id: string;
  customerName: string;
  phone?: string;
  billingAddress?: string;
  psid?: string;
  storeId: string;
  createdAt: string;
}

export interface CreateCustomerRequest {
  customerName: string;
  phone?: string;
  billingAddress?: string;
}

export interface UpdateCustomerRequest {
  customerName?: string;
  phone?: string;
  billingAddress?: string;
}

// Order
export type OrderStatus = 
  | 'Pending' 
  | 'Accepted' 
  | 'Shipped' 
  | 'Delivered' 
  | 'Rejected' 
  | 'Cancelled' 
  | 'Refunded';

export interface OrderProduct {
  orderId: string;
  productId: string;
  productName?: string;
  quantity: number;
  unitPrice: number;
}

export interface Order {
  id: string;
  customerId: string;
  customerName?: string;
  customer?: Customer;
  totalPrice: number;
  status: OrderStatus;
  statusDisplayName: string;
  orderSource?: 'Chatbot' | 'Manual';
  items?: OrderProduct[];
  storeId: string;
  createdAt: string;
}

export interface CreateOrderRequest {
  customerId: string;
  items: { productId: string; quantity: number }[];
}

// Campaign
export type CampaignStage = 
  | 'Draft' 
  | 'InReview' 
  | 'Scheduled' 
  | 'Ready' 
  | 'Published';

export interface Campaign {
  id: string;
  campaignName: string;
  campaignDescription?: string;
  campaignStage: CampaignStage;
  campaignBannerUrl?: string;
  goal?: string;
  targetAudience?: string;
  scheduledStartAt?: string;
  scheduledEndAt?: string;
  isSchedulingEnabled?: boolean;
  assignedProductId?: string;
  assignedProductName?: string;
  createdByUserId: string;
  createdByUserName?: string;
  storeId: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateCampaignRequest {
  campaignName: string;
  campaignDescription?: string;
  campaignStage: CampaignStage;
  goal?: string;
  targetAudience?: string;
  isSchedulingEnabled?: boolean;
  scheduledStartAt?: string;
  scheduledEndAt?: string;
  assignedProductId?: string;
}

export interface UpdateCampaignRequest {
  campaignName?: string;
  campaignDescription?: string;
  campaignStage?: CampaignStage;
  goal?: string;
  targetAudience?: string;
  scheduledStartAt?: string;
  scheduledEndAt?: string;
  isSchedulingEnabled?: boolean;
  assignedProductId?: string;
}

// Campaign Post
export type PostPublishStatus = 'Pending' | 'Publishing' | 'Published' | 'Failed';

export interface CampaignPost {
  id: string;
  campaignId: string;
  campaignName?: string;
  postCaption: string;
  postImageUrl?: string;
  scheduledAt?: string;
  publishStatus: PostPublishStatus;
  publishedAt?: string;
  lastPublishError?: string;
  platforms?: CampaignPostPlatform[];
  createdAt: string;
}

export interface CampaignPostPlatform {
  id: string;
  campaignPostId: string;
  platformId: string;
  platformName: string;
  externalPostId?: string;
  publishStatus: PostPublishStatus;
  scheduledAt?: string;
  publishedAt?: string;
  errorMessage?: string;
}

// CreateCampaignPostRequest - NO platformIds!
// Backend resolves platforms using storeId from the campaign
export interface CreateCampaignPostRequest {
  campaignId: string;
  postCaption: string;
  postImageUrl?: string;
  scheduledAt?: string;
  // platformIds removed - backend handles platform resolution
}

// Social Platform
export interface SocialPlatform {
  id: string;
  platformName: string;
  externalPageID?: string;
  pageName?: string;
  accessToken?: string;
  isConnected: boolean;
  storeId: string;
  createdAt: string;
  updatedAt?: string;
}

// API Response Wrappers
export interface ApiResponse<T> {
  data: T;
  message?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

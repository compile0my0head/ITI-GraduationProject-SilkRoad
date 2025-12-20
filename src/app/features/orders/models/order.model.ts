// NOTE: Use core/models/api.models.ts for all Order-related types
// This file is kept for backwards compatibility but re-exports from the central models

export type { 
  Order, 
  OrderProduct, 
  OrderStatus, 
  CreateOrderRequest 
} from '../../../core/models/api.models';

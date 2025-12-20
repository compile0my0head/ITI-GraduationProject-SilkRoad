export interface Customer {
  customerID: number;
  storeID: number;
  customerName: string;
  billingAddress: string;
  phone: string;
  createdAt: Date;
}

export interface CreateCustomerRequest {
  customerName: string;
  billingAddress: string;
  phone: string;
}

export interface UpdateCustomerRequest {
  customerName?: string;
  billingAddress?: string;
  phone?: string;
}

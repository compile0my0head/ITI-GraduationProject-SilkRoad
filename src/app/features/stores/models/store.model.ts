export interface Store {
  storeID: number;
  storeName: string;
  ownerUserID: number;
  createdAt: Date;
}

export interface CreateStoreRequest {
  storeName: string;
}

export interface UpdateStoreRequest {
  storeName?: string;
}

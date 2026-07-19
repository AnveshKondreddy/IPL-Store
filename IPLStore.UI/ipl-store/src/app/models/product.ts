export interface ProductListItem {
  id: number;
  name: string;
  type: string;
  franchiseName: string;
  price: number;
  imageUrl?: string;
}

export interface ProductDetails extends ProductListItem {
  description: string;
  stockQty: number;
  isActive: boolean;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

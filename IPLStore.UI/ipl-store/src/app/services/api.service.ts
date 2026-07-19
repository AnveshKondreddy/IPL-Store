import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult, ProductDetails, ProductListItem } from '../models/product';
import { Cart } from '../models/cart';
import { Order } from '../models/order';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = '/api';
  private readonly http = inject(HttpClient);

  // Products
  searchProducts(filters: {
    search?: string;
    type?: string;
    franchise?: string;
    page?: number;
    pageSize?: number;
  }): Observable<PagedResult<ProductListItem>> {
    let params = new HttpParams();
    if (filters.search) params = params.set('search', filters.search);
    if (filters.type) params = params.set('type', filters.type);
    if (filters.franchise) params = params.set('franchise', filters.franchise);
    if (filters.page) params = params.set('page', filters.page);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);

    return this.http.get<PagedResult<ProductListItem>>(`${this.baseUrl}/products`, { params });
  }

  getProduct(id: number): Observable<ProductDetails> {
    return this.http.get<ProductDetails>(`${this.baseUrl}/products/${id}`);
  }

  // Cart
  getCart(userId: string): Observable<Cart> {
    return this.http.get<Cart>(`${this.baseUrl}/cart/${userId}`);
  }

  addToCart(userId: string, productId: number, quantity: number): Observable<Cart> {
    return this.http.post<Cart>(`${this.baseUrl}/cart/${userId}/items`, { productId, quantity });
  }

  updateCartItem(userId: string, itemId: number, quantity: number): Observable<Cart> {
    return this.http.put<Cart>(`${this.baseUrl}/cart/${userId}/items/${itemId}`, { quantity });
  }

  removeCartItem(userId: string, itemId: number): Observable<Cart> {
    return this.http.delete<Cart>(`${this.baseUrl}/cart/${userId}/items/${itemId}`);
  }

  // Orders
  checkout(userId: string): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/orders/checkout/${userId}`, {});
  }

  getOrderHistory(userId: string, page = 1, pageSize = 10): Observable<PagedResult<Order>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<Order>>(`${this.baseUrl}/orders/${userId}`, { params });
  }

  getOrder(userId: string, orderId: number): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/orders/${userId}/${orderId}`);
  }
}

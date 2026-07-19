import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./pages/home/home').then(m => m.HomeComponent) },
  { path: 'products/:id', loadComponent: () => import('./pages/product-detail/product-detail').then(m => m.ProductDetailComponent) },
  { path: 'cart', loadComponent: () => import('./pages/cart/cart').then(m => m.CartComponent) },
  { path: 'orders', loadComponent: () => import('./pages/orders/orders').then(m => m.OrdersComponent) },
  { path: '**', redirectTo: '' }
];

import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./pages/login/login').then(m => m.LoginComponent) },
  { path: '', loadComponent: () => import('./pages/home/home').then(m => m.HomeComponent), canActivate: [authGuard] },
  { path: 'products/:id', loadComponent: () => import('./pages/product-detail/product-detail').then(m => m.ProductDetailComponent), canActivate: [authGuard] },
  { path: 'cart', loadComponent: () => import('./pages/cart/cart').then(m => m.CartComponent), canActivate: [authGuard] },
  { path: 'orders', loadComponent: () => import('./pages/orders/orders').then(m => m.OrdersComponent), canActivate: [authGuard] },
  { path: '**', redirectTo: '' }
];

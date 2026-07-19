import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { Cart } from '../../models/cart';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CurrencyPipe, RouterLink, MatTableModule, MatButtonModule, MatCardModule, MatIconModule],
  templateUrl: './cart.html',
  styleUrl: './cart.scss'
})
export class CartComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly snackBar = inject(MatSnackBar);

  readonly cart = signal<Cart | null>(null);
  readonly loading = signal(true);
  readonly checkingOut = signal(false);
  readonly displayedColumns = ['product', 'price', 'quantity', 'total', 'actions'];

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.loading.set(true);
    this.api.getCart(this.auth.username()!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (c) => { this.cart.set(c); this.loading.set(false); },
        error: () => { this.loading.set(false); }
      });
  }

  updateQuantity(productId: number, qty: number): void {
    if (qty < 1) return;
    this.api.updateCartItem(this.auth.username()!, productId, qty)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadCart());
  }

  removeItem(productId: number): void {
    this.api.removeCartItem(this.auth.username()!, productId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadCart());
  }

  checkout(): void {
    this.checkingOut.set(true);
    this.api.checkout(this.auth.username()!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (order) => {
          this.snackBar.open('Order placed successfully!', 'View', { duration: 4000 })
            .onAction().subscribe(() => this.router.navigate(['/orders']));
          this.router.navigate(['/orders'], { queryParams: { placed: order.id } });
        },
        error: (err) => {
          this.snackBar.open(err.error.message || 'Checkout failed. Please try again.', 'OK', { duration: 3000 });
          this.checkingOut.set(false);
        }
      });
  }
}

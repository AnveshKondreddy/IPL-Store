import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { ProductDetails } from '../../models/product';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [RouterLink, CurrencyPipe, FormsModule, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss'
})
export class ProductDetailComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly snackBar = inject(MatSnackBar);

  readonly product = signal<ProductDetails | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly quantity = signal(1);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.api.getProduct(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (p) => { this.product.set(p); this.loading.set(false); },
        error: () => { this.error.set('Product not found.'); this.loading.set(false); }
      });
  }

  adjustQuantity(delta: number): void {
    const next = this.quantity() + delta;
    if (next >= 1) this.quantity.set(next);
  }

  addToCart(): void {
    const p = this.product();
    if (!p) return;

    this.api.addToCart(this.auth.username()!, p.id, this.quantity())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.snackBar.open(this.quantity() + ' x ' + p.name + ' added to cart', 'OK', { duration: 2000 });
        },
        error: () => {
          this.snackBar.open('Failed to add item. Please try again.', 'OK', { duration: 3000 });
        }
      });
  }
}

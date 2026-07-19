import { Component, DestroyRef, inject, OnInit, signal, computed } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../../services/api.service';
import { ProductListItem } from '../../models/product';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    FormsModule, RouterLink, CurrencyPipe,
    MatCardModule, MatButtonModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatChipsModule, MatIconModule
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class HomeComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly snackBar = inject(MatSnackBar);

  readonly products = signal<ProductListItem[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly quantities = signal<Record<number, number>>({});

  readonly search = signal('');
  readonly type = signal('');
  readonly franchise = signal('');
  readonly page = signal(1);
  readonly pageSize = 12;

  readonly totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize));

  readonly franchises = [
    { name: 'Mumbai Indians', code: 'MI' },
    { name: 'Chennai Super Kings', code: 'CSK' },
    { name: 'Royal Challengers Bengaluru', code: 'RCB' },
    { name: 'Kolkata Knight Riders', code: 'KKR' },
  ];
  readonly types = ['Jersey', 'Cap', 'Flag', 'Mug', 'Hoodie', 'T-Shirt'];

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading.set(true);
    this.error.set('');

    this.api.searchProducts({
      search: this.search() || undefined,
      type: this.type() || undefined,
      franchise: this.franchise() || undefined,
      page: this.page(),
      pageSize: this.pageSize
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load products. Please try again.');
        this.loading.set(false);
      }
    });
  }

  onSearch(): void {
    this.page.set(1);
    this.loadProducts();
  }

  clearFilters(): void {
    this.search.set('');
    this.type.set('');
    this.franchise.set('');
    this.page.set(1);
    this.loadProducts();
  }

  goToPage(p: number): void {
    if (p < 1 || p > this.totalPages()) return;
    this.page.set(p);
    this.loadProducts();
  }

  getQuantity(productId: number): number {
    return this.quantities()[productId] ?? 1;
  }

  setQuantity(productId: number, qty: number): void {
    if (qty >= 1) this.quantities.update(q => ({ ...q, [productId]: qty }));
  }

  addToCart(product: ProductListItem): void {
    const qty = this.getQuantity(product.id);
    this.api.addToCart('default-user', product.id, qty)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.snackBar.open(`${qty} � ${product.name} added to cart`, 'OK', { duration: 2000 });
          this.quantities.update(q => {
            const { [product.id]: _, ...rest } = q;
            return rest;
          });
        },
        error: () => {
          this.snackBar.open('Failed to add item. Please try again.', 'OK', { duration: 3000 });
        }
      });
  }
}

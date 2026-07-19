import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { ApiService } from '../../services/api.service';
import { Order } from '../../models/order';
import { PagedResult } from '../../models/product';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink, MatExpansionModule, MatTableModule, MatButtonModule, MatCardModule],
  templateUrl: './orders.html',
  styleUrl: './orders.scss'
})
export class OrdersComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly orders = signal<Order[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);
  readonly placedOrderId = signal<number | null>(null);
  readonly page = signal(1);
  readonly pageSize = 10;
  readonly itemColumns = ['product', 'quantity', 'price', 'total'];

  ngOnInit(): void {
    const placed = this.route.snapshot.queryParamMap.get('placed');
    if (placed) this.placedOrderId.set(Number(placed));
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading.set(true);
    this.api.getOrderHistory('default-user', this.page(), this.pageSize)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result: PagedResult<Order>) => {
          this.orders.set(result.items);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount() / this.pageSize);
  }

  goToPage(p: number): void {
    if (p < 1 || p > this.totalPages) return;
    this.page.set(p);
    this.loadOrders();
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ALL_STATUSES, OrderStatus } from '../../core/models/order-status.model';
import { OrderStoreService } from '../../core/services/order-store.service';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.css',
})
export class OrderListComponent implements OnInit {
  readonly search = signal('');
  readonly visibleOrders = computed(() => {
    const query = this.search().trim().toLowerCase();
    return this.store
      .orders()
      .filter((order) =>
        `${order.externalReference} ${order.customerName} ${order.customerEmail}`
          .toLowerCase()
          .includes(query),
      );
  });

  readonly allStatuses = ALL_STATUSES;

  constructor(
    readonly store: OrderStoreService,
    route: ActivatedRoute,
  ) {
    route.queryParamMap
      .pipe(takeUntilDestroyed())
      .subscribe((params) => this.search.set(params.get('q') || ''));
  }

  ngOnInit(): void {
    void this.store.refresh();
  }

  async onFilterChange(value: string): Promise<void> {
    await this.store.setStatusFilter((value || null) as OrderStatus | null);
  }

  async refresh(): Promise<void> {
    await this.store.refresh();
  }
}

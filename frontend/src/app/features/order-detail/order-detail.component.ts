import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ALLOWED_NEXT_STATUSES, OrderStatus } from '../../core/models/order-status.model';
import { OrderStoreService } from '../../core/services/order-store.service';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.css',
})
export class OrderDetailComponent {
  readonly changingStatus = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly loadError = signal<string | null>(null);

  constructor(
    readonly store: OrderStoreService,
    private readonly route: ActivatedRoute,
  ) {
    this.route.paramMap.pipe(takeUntilDestroyed()).subscribe((params) => {
      void this.loadOrder(params.get('id')!);
    });
  }

  async loadOrder(id = this.route.snapshot.paramMap.get('id')!): Promise<void> {
    this.loadError.set(null);
    this.errorMessage.set(null);
    try {
      await this.store.selectOrder(id);
    } catch (err) {
      this.loadError.set(err instanceof Error ? err.message : 'Could not load this order.');
    }
  }

  get allowedNextStatuses(): OrderStatus[] {
    const order = this.store.selectedOrder();
    return order ? ALLOWED_NEXT_STATUSES[order.status] : [];
  }

  async changeStatus(status: OrderStatus): Promise<void> {
    const order = this.store.selectedOrder();
    if (!order) {
      return;
    }

    this.errorMessage.set(null);
    this.changingStatus.set(true);
    try {
      await this.store.changeStatus(order.id, status);
    } catch (err) {
      this.errorMessage.set(err instanceof Error ? err.message : 'Failed to update status.');
    } finally {
      this.changingStatus.set(false);
    }
  }
}

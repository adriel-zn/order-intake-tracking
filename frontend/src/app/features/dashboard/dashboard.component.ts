import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { OrderService } from '../../core/services/order.service';
import type { OrderSummary } from '../../core/models/order-summary.model';
import { IconComponent } from '../../shared/icon.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, IconComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {
  private readonly service = inject(OrderService);
  readonly orders = signal<OrderSummary[]>([]);
  readonly loading = signal(true);
  readonly error = signal(false);
  readonly days = Array.from({ length: 7 }, (_, index) => {
    const date = new Date();
    date.setHours(0, 0, 0, 0);
    date.setDate(date.getDate() - 6 + index);
    return date;
  });
  readonly recent = computed(() =>
    [...this.orders()]
      .sort((a, b) => Date.parse(b.updatedAt) - Date.parse(a.updatedAt))
      .slice(0, 5),
  );
  readonly received = computed(() => this.dailyCounts(this.orders()));
  readonly fulfilled = computed(() =>
    this.dailyCounts(this.orders().filter((order) => order.status === 'Fulfilled')),
  );
  readonly maximum = computed(() => Math.max(4, ...this.received()));
  readonly metrics = computed(() => [
    { label: 'Total orders', value: this.orders().length, icon: 'box', values: this.received() },
    {
      label: 'Awaiting confirmation',
      value: this.orders().filter((o) => o.status === 'Pending').length,
      icon: 'clock',
      values: this.dailyCounts(this.orders().filter((o) => o.status === 'Pending')),
    },
    {
      label: 'Orders fulfilled',
      value: this.orders().filter((o) => o.status === 'Fulfilled').length,
      icon: 'check',
      values: this.fulfilled(),
    },
  ]);

  ngOnInit(): void {
    void this.refresh();
  }

  async refresh(): Promise<void> {
    this.loading.set(true);
    this.error.set(false);
    try {
      this.orders.set(await firstValueFrom(this.service.listOrders()));
    } catch {
      this.error.set(true);
    } finally {
      this.loading.set(false);
    }
  }

  private dailyCounts(orders: OrderSummary[]): number[] {
    return this.days.map(
      (day) =>
        orders.filter((order) => new Date(order.createdAt).toDateString() === day.toDateString())
          .length,
    );
  }

  points(values: number[]): string {
    return values
      .map((value, index) => `${48 + index * 82},${218 - (value / this.maximum()) * 180}`)
      .join(' ');
  }

  sparkline(values: number[]): string {
    const maximum = Math.max(1, ...values);
    return (
      'M0 56 ' +
      values.map((value, index) => `L${index * 40} ${52 - (value / maximum) * 42}`).join(' ') +
      ' L240 56 Z'
    );
  }
}

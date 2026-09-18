import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import type { CreateOrderRequest } from '../models/create-order-request.model';
import type { Order } from '../models/order.model';
import type { OrderStatus } from '../models/order-status.model';
import type { OrderSummary } from '../models/order-summary.model';
import type { ProblemDetails } from '../models/problem-details.model';
import { OrderService } from './order.service';

export interface CreateOrderOutcome {
  order: Order;
  /** True when the server reported this as an idempotent replay of an
   *  already-submitted order rather than a brand-new one (HTTP 200 vs 201). */
  wasDuplicate: boolean;
}

/**
 * Central UI state for the orders feature, built on Angular signals.
 *
 * Components read state via the exposed readonly signals and act through the
 * methods here; this keeps HTTP calls, error normalization, and "which order
 * is currently selected" in one place instead of duplicated per component.
 */
@Injectable({ providedIn: 'root' })
export class OrderStoreService {
  private readonly ordersSignal = signal<OrderSummary[]>([]);
  private readonly selectedOrderSignal = signal<Order | null>(null);
  private readonly listLoadingSignal = signal(false);
  private readonly detailLoadingSignal = signal(false);
  private readonly listErrorSignal = signal<string | null>(null);
  private readonly statusFilterSignal = signal<OrderStatus | null>(null);

  readonly orders = this.ordersSignal.asReadonly();
  readonly selectedOrder = this.selectedOrderSignal.asReadonly();
  readonly listLoading = this.listLoadingSignal.asReadonly();
  readonly detailLoading = this.detailLoadingSignal.asReadonly();
  readonly listError = this.listErrorSignal.asReadonly();
  readonly statusFilter = this.statusFilterSignal.asReadonly();

  readonly orderCount = computed(() => this.ordersSignal().length);

  constructor(private readonly orderService: OrderService) {}

  async refresh(): Promise<void> {
    this.listLoadingSignal.set(true);
    this.listErrorSignal.set(null);
    try {
      const orders = await firstValueFrom(this.orderService.listOrders(this.statusFilterSignal()));
      this.ordersSignal.set(orders);
    } catch (err) {
      this.listErrorSignal.set(toErrorMessage(err));
    } finally {
      this.listLoadingSignal.set(false);
    }
  }

  async setStatusFilter(status: OrderStatus | null): Promise<void> {
    this.statusFilterSignal.set(status);
    await this.refresh();
  }

  async selectOrder(id: string): Promise<void> {
    this.detailLoadingSignal.set(true);
    try {
      const order = await firstValueFrom(this.orderService.getOrder(id));
      this.selectedOrderSignal.set(order);
    } catch (err) {
      this.selectedOrderSignal.set(null);
      throw new Error(toErrorMessage(err));
    } finally {
      this.detailLoadingSignal.set(false);
    }
  }

  clearSelection(): void {
    this.selectedOrderSignal.set(null);
  }

  /** Throws a human-readable Error on failure (validation or genuine conflict). */
  async submitOrder(request: CreateOrderRequest): Promise<CreateOrderOutcome> {
    try {
      const response = await firstValueFrom(this.orderService.createOrder(request));
      // The API returns 201 for a brand-new order and 200 when the same
      // externalReference was already submitted with identical details.
      const wasDuplicate = response.status === 200;
      await this.refresh();
      return { order: response.body as Order, wasDuplicate };
    } catch (err) {
      throw new Error(toErrorMessage(err));
    }
  }

  /** Throws a human-readable Error on failure (not found or invalid transition). */
  async changeStatus(id: string, status: OrderStatus): Promise<Order> {
    try {
      const updated = await firstValueFrom(this.orderService.updateStatus(id, status));
      this.selectedOrderSignal.set(updated);
      await this.refresh();
      return updated;
    } catch (err) {
      throw new Error(toErrorMessage(err));
    }
  }
}

/** Turns an HttpErrorResponse (ProblemDetails or validation problem) into a
 *  single readable string for display in the UI. */
function toErrorMessage(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    const body = err.error as ProblemDetails | undefined;

    if (body?.errors) {
      const messages = Object.values(body.errors).flat();
      if (messages.length > 0) {
        return messages.join(' ');
      }
    }

    if (body?.detail) {
      return body.detail;
    }

    if (body?.title) {
      return body.title;
    }

    if (err.status === 0) {
      return 'Could not reach the server. Is the API running?';
    }

    return `Request failed (HTTP ${err.status}).`;
  }

  return err instanceof Error ? err.message : 'An unexpected error occurred.';
}

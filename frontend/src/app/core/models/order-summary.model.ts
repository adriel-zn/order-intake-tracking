import type { OrderStatus } from './order-status.model';

/** Lightweight projection used for the order list. */
export interface OrderSummary {
  id: string;
  externalReference: string;
  customerName: string;
  customerEmail: string;
  status: OrderStatus;
  currency: string;
  total: number;
  lineItemCount: number;
  createdAt: string;
  updatedAt: string;
}

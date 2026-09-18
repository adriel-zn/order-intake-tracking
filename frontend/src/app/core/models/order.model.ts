import type { Customer } from './customer.model';
import type { LineItem } from './line-item.model';
import type { OrderStatus } from './order-status.model';

export interface Order {
  id: string;
  externalReference: string;
  customer: Customer;
  lineItems: LineItem[];
  currency: string;
  notes: string | null;
  status: OrderStatus;
  subtotal: number;
  total: number;
  createdAt: string;
  updatedAt: string;
}

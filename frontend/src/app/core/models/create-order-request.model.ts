import type { CreateOrderLineItemRequest } from './create-order-line-item-request.model';

export interface CreateOrderRequest {
  externalReference: string;
  customer: { email: string; name: string };
  currency: string;
  notes?: string | null;
  lineItems: CreateOrderLineItemRequest[];
}

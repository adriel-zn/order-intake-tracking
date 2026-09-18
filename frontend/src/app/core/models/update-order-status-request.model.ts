import type { OrderStatus } from './order-status.model';

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
}

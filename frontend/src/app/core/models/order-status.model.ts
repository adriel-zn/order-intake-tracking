/** Mirrors OrderIntake.Api.Models.OrderStatus (serialized as strings by the API). */
export type OrderStatus = 'Pending' | 'Confirmed' | 'Fulfilled' | 'Cancelled';

export const ALL_STATUSES: OrderStatus[] = ['Pending', 'Confirmed', 'Fulfilled', 'Cancelled'];

/** Allowed next statuses for a given current status — mirrors OrderStatusTransitions
 *  on the backend. Used only to drive which action buttons are shown; the server
 *  is always the source of truth and re-validates every change. */
export const ALLOWED_NEXT_STATUSES: Record<OrderStatus, OrderStatus[]> = {
  Pending: ['Confirmed', 'Cancelled'],
  Confirmed: ['Fulfilled', 'Cancelled'],
  Fulfilled: [],
  Cancelled: [],
};

import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { CreateOrderRequest } from '../models/create-order-request.model';
import type { Order } from '../models/order.model';
import type { OrderStatus } from '../models/order-status.model';
import type { OrderSummary } from '../models/order-summary.model';
import type { UpdateOrderStatusRequest } from '../models/update-order-status-request.model';

/**
 * Thin wrapper around the Order Intake API. Kept free of UI/error-presentation
 * concerns — that lives in OrderStoreService — so it's easy to unit test or
 * swap out in isolation.
 */
@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly baseUrl = `${environment.apiBaseUrl}/orders`;

  constructor(private readonly http: HttpClient) {}

  /**
   * Submits a new order. Note the API itself dedupes by externalReference:
   * a resubmission of an identical order returns the existing order with
   * HTTP 200 (vs 201 for a genuinely new order), and a resubmission with
   * different details is rejected with 409 (surfaced as an error).
   * The full response is returned so callers can tell 200 from 201.
   */
  createOrder(request: CreateOrderRequest): Observable<HttpResponse<Order>> {
    return this.http.post<Order>(this.baseUrl, request, { observe: 'response' });
  }

  listOrders(status?: OrderStatus | null): Observable<OrderSummary[]> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<OrderSummary[]>(this.baseUrl, { params });
  }

  getOrder(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/${id}`);
  }

  updateStatus(id: string, status: OrderStatus): Observable<Order> {
    const body: UpdateOrderStatusRequest = { status };
    return this.http.patch<Order>(`${this.baseUrl}/${id}/status`, body);
  }
}

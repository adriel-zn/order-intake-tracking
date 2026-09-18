import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { routes } from './app.routes';
import { OrderListComponent } from './features/order-list/order-list.component';
import { OrderFormComponent } from './features/order-form/order-form.component';
import { OrderDetailComponent } from './features/order-detail/order-detail.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';

describe('Order screens', () => {
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('opens the dashboard with real order counts and links', async () => {
    const harness = await RouterTestingHarness.create();
    const dashboard = await harness.navigateByUrl('/', DashboardComponent);
    const now = new Date().toISOString();
    http
      .expectOne((req) => req.method === 'GET')
      .flush([
        {
          id: 'one',
          externalReference: 'PO-1',
          status: 'Pending',
          customerName: 'Acme',
          createdAt: now,
          updatedAt: now,
        },
        {
          id: 'two',
          externalReference: 'PO-2',
          status: 'Fulfilled',
          customerName: 'Example',
          createdAt: now,
          updatedAt: now,
        },
      ]);
    await new Promise<void>((resolve) => setTimeout(resolve, 0));
    harness.detectChanges();
    expect(dashboard.metrics().map((metric) => metric.value)).toEqual([2, 1, 1]);
    expect(dashboard.received()[6]).toBe(2);
    expect(dashboard.fulfilled()[6]).toBe(1);
    expect(harness.routeNativeElement?.querySelector('a[href="/orders/one"]')).not.toBeNull();
  });

  it('applies header search parameters when already on the order screen', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/orders?q=Acme', OrderListComponent);
    http
      .expectOne((req) => req.method === 'GET')
      .flush([
        {
          id: 'one',
          externalReference: 'PO-1',
          customerName: 'Acme',
          customerEmail: 'a@example.com',
        },
        {
          id: 'two',
          externalReference: 'PO-2',
          customerName: 'Example',
          customerEmail: 'b@example.com',
        },
      ]);
    await new Promise<void>((resolve) => setTimeout(resolve, 0));
    const list = await harness.navigateByUrl('/orders?q=PO-2', OrderListComponent);
    expect(list.visibleOrders().map((order) => order.id)).toEqual(['two']);
  });

  it('opens the directory and separates the new order screen', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/orders', OrderListComponent);
    http.expectOne((req) => req.method === 'GET').flush([]);
    await harness.fixture.whenStable();
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('Your orders start here');
    await harness.navigateByUrl('/orders/new', OrderFormComponent);
    expect(harness.routeNativeElement?.querySelector('form')).not.toBeNull();
    expect(harness.routeNativeElement?.textContent).not.toContain('Order directory');
  });

  it('loads an order from its URL and presents a recoverable error', async () => {
    const harness = await RouterTestingHarness.create();
    const detail = await harness.navigateByUrl('/orders/missing', OrderDetailComponent);
    http
      .expectOne((req) => req.url.endsWith('/missing'))
      .flush({ detail: 'Order not found.' }, { status: 404, statusText: 'Not Found' });
    // Drain the service rejection and the component's async error handler.
    await new Promise<void>((resolve) => setTimeout(resolve, 0));
    await harness.fixture.whenStable();
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('Order not found.');
    expect(harness.routeNativeElement?.textContent).toContain('Try again');
    const retry = detail.loadOrder();
    http
      .expectOne((req) => req.url.endsWith('/missing'))
      .flush({
        id: 'missing',
        externalReference: 'PO-1001',
        customer: { name: 'Acme', email: 'buyer@example.com' },
        status: 'Pending',
        lineItems: [],
        total: 0,
        subtotal: 0,
        currency: 'USD',
        createdAt: '2026-09-18T10:00:00Z',
        updatedAt: '2026-09-18T10:00:00Z',
      });
    await retry;
    harness.detectChanges();
    expect(harness.routeNativeElement?.textContent).toContain('PO-1001');
    expect(harness.routeNativeElement?.textContent).toContain('Mark as Confirmed');
  });
});

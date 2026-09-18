import { Routes } from '@angular/router';
import { OrderListComponent } from './features/order-list/order-list.component';
import { OrderFormComponent } from './features/order-form/order-form.component';
import { OrderDetailComponent } from './features/order-detail/order-detail.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', component: DashboardComponent, title: 'Dashboard | Order Intake' },
  { path: 'orders', component: OrderListComponent, title: 'All orders | Order Intake' },
  { path: 'orders/new', component: OrderFormComponent, title: 'New order | Order Intake' },
  { path: 'orders/:id', component: OrderDetailComponent, title: 'Order details | Order Intake' },
  { path: '**', redirectTo: 'orders' },
];

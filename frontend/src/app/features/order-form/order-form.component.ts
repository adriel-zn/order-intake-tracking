import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { OrderStoreService } from '../../core/services/order-store.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-order-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './order-form.component.html',
  styleUrl: './order-form.component.css',
})
export class OrderFormComponent {
  // Declared before `form` (and using inject() rather than constructor
  // parameters) so they're guaranteed to be initialized before the `form`
  // field initializer below runs.
  private readonly fb = inject(FormBuilder);
  private readonly orderStore = inject(OrderStoreService);

  readonly submitting = signal(false);
  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly submittedOrderId = signal<string | null>(null);

  readonly form = this.fb.group({
    externalReference: ['', [Validators.required, Validators.maxLength(200)]],
    customerEmail: ['', [Validators.required, Validators.email]],
    customerName: ['', [Validators.required, Validators.maxLength(200)]],
    currency: ['USD', [Validators.required, Validators.pattern(/^[A-Za-z]{3}$/)]],
    notes: ['', [Validators.maxLength(2000)]],
    lineItems: this.fb.array(
      [this.buildLineItem()],
      [Validators.required, Validators.minLength(1)],
    ),
  });

  get lineItems(): FormArray {
    return this.form.get('lineItems') as FormArray;
  }

  asGroup(control: AbstractControl): FormGroup {
    return control as FormGroup;
  }

  private buildLineItem() {
    return this.fb.group({
      code: ['', [Validators.required, Validators.maxLength(100)]],
      name: ['', [Validators.required, Validators.maxLength(200)]],
      quantity: [1, [Validators.required, Validators.min(1), Validators.pattern(/^\d+$/)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
    });
  }

  addLineItem(): void {
    this.lineItems.push(this.buildLineItem());
  }

  removeLineItem(index: number): void {
    if (this.lineItems.length > 1) {
      this.lineItems.removeAt(index);
    }
  }

  /** Server-computed preview shown to the rep before submitting, purely for
   *  UX confirmation — the API recomputes this independently and is authoritative. */
  get previewTotal(): number {
    return this.lineItems.controls.reduce((sum, group) => {
      const qty = Number(group.get('quantity')?.value) || 0;
      const price = Number(group.get('unitPrice')?.value) || 0;
      return sum + qty * price;
    }, 0);
  }

  async submit(): Promise<void> {
    if (this.submitting()) return;
    this.submittedOrderId.set(null);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMessage.set('Please fix the highlighted fields before submitting.');
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);
    try {
      const { order, wasDuplicate } = await this.orderStore.submitOrder({
        externalReference: value.externalReference!.trim(),
        customer: {
          email: value.customerEmail!.trim(),
          name: value.customerName!.trim(),
        },
        currency: value.currency!.trim(),
        notes: value.notes?.trim() || null,
        lineItems: (value.lineItems ?? []).map((li) => ({
          code: li.code!.trim(),
          name: li.name!.trim(),
          quantity: Number(li.quantity),
          unitPrice: Number(li.unitPrice),
        })),
      });

      this.successMessage.set(
        wasDuplicate
          ? `Reference "${order.externalReference}" was already submitted — showing the existing order (no duplicate created).`
          : `Order "${order.externalReference}" created successfully.`,
      );
      this.submittedOrderId.set(order.id);
      this.resetForm();
    } catch (err) {
      this.errorMessage.set(err instanceof Error ? err.message : 'Failed to submit order.');
    } finally {
      this.submitting.set(false);
    }
  }

  private resetForm(): void {
    this.form.reset({ currency: 'USD', notes: '' });
    while (this.lineItems.length > 0) {
      this.lineItems.removeAt(0);
    }
    this.lineItems.push(this.buildLineItem());
  }
}

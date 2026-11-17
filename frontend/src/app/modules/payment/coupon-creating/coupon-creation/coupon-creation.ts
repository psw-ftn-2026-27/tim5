import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, max, min, required, validate } from '@angular/forms/signals';
import { serverMessage } from '../../../../shared/util/server-message';
import { CouponDto, MyProductDto } from '../../api/payment-api-types';
import { CouponCreating } from '../coupon-creating';

type CouponScope = 'all' | 'product';

@Component({
  imports: [FormField],
  selector: 'app-coupon-creation',
  styleUrl: './coupon-creation.scss',
  templateUrl: './coupon-creation.html',
})
export class CouponCreation {
  private readonly couponCreating = inject(CouponCreating);
  protected readonly utcToday = new Date().toISOString().slice(0, 10);

  protected readonly products = httpResource<MyProductDto[]>(() => '/api/payment/products/mine');
  protected readonly couponId = signal<string | null>(null);
  protected readonly coupon = httpResource<CouponDto>(() => {
    const id = this.couponId();
    return id ? `/api/payment/coupons/${id}` : undefined;
  });

  protected readonly form = form(
    signal({
      discountPercentage: null as number | null,
      expiresAt: '',
      scope: 'all' as CouponScope,
      productId: '',
    }),
    (path) => {
      required(path.discountPercentage, { message: 'Discount percentage is required.' });
      min(path.discountPercentage, 1, { message: 'Discount must be at least 1%.' });
      max(path.discountPercentage, 100, { message: 'Discount cannot exceed 100%.' });
      validate(path.discountPercentage, ({ value }) =>
        value() === null || Number.isInteger(value())
          ? undefined
          : { kind: 'integer', message: 'Discount must be a whole number.' },
      );
      required(path.expiresAt, { message: 'Expiry date is required.' });
      validate(path.expiresAt, ({ value }) =>
        value() === '' || value() >= this.utcToday
          ? undefined
          : { kind: 'minDate', message: 'Expiry date cannot be before today (UTC).' },
      );
    },
  );

  protected readonly showForm = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly productError = signal<string | null>(null);
  protected readonly copied = signal(false);
  protected readonly canCreate = computed(() =>
    this.form().valid() &&
    !this.pending() &&
    (this.form.scope().value() === 'all' || this.form.productId().value() !== ''),
  );

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    const discountPercentage = this.form.discountPercentage().value();
    const expiresAt = this.form.expiresAt().value();
    const scope = this.form.scope().value();
    const productId = this.form.productId().value();
    if (discountPercentage === null || expiresAt === '' || (scope === 'product' && productId === '')) {
      this.productError.set(scope === 'product' && productId === '' ? 'Choose a product.' : null);
      return;
    }

    this.pending.set(true);
    this.error.set(null);
    this.productError.set(null);
    this.copied.set(false);
    try {
      const created = await this.couponCreating.createCoupon({
        discountPercentage,
        expiresAt,
        productId: scope === 'product' ? productId : null,
      });
      this.couponId.set(created.id);
      this.showForm.set(false);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the coupon. Check the details and try again.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async copyCode(code: string): Promise<void> {
    try {
      await navigator.clipboard.writeText(code);
      this.copied.set(true);
    } catch {
      this.error.set('Could not copy the coupon code. Select and copy it manually.');
    }
  }

  protected createAnother(): void {
    this.couponId.set(null);
    this.copied.set(false);
  }

  protected retryLoadingCoupon(): void {
    this.coupon.reload();
  }

  protected productName(productId: string): string {
    return this.products.value()?.find((product) => product.productId === productId)?.name ?? productId;
  }
}

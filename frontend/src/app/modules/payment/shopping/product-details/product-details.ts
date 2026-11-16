import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { ProductDto } from '../../api/payment-api-types';
import { Shopping } from '../shopping';

@Component({
  selector: 'app-product-details',
  styleUrl: './product-details.scss',
  templateUrl: './product-details.html',
})
export class ProductDetails {
  private readonly shopping = inject(Shopping);
  private readonly router = inject(Router);
  private readonly auth = inject(Auth);

  readonly entityId = input.required<string>();

  protected readonly product = httpResource<ProductDto>(
    () => `/api/payment/products/${this.entityId()}`,
  );

  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  protected readonly hasNoProduct = computed(() => {
    const failure = this.product.error();
    return failure instanceof HttpErrorResponse && failure.status === 404;
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async addToCart(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.shopping.addToCart({ entityId: this.entityId() });
      await this.router.navigate(['/payment/cart']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'The tour was not added to the cart.'));
    } finally {
      this.pending.set(false);
    }
  }
}

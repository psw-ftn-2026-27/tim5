import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AddToCartDto, CheckoutBenefitsDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/cart/items';
const CHECKOUT_URL = '/api/payment/cart/checkout';

@Injectable({ providedIn: 'root' })
export class Shopping {
  private readonly http = inject(HttpClient);

  async addToCart(dto: AddToCartDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(BASE_URL, dto));
  }

  async removeFromCart(entityId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${entityId}`));
  }

  async checkout(benefits: CheckoutBenefitsDto | null): Promise<string | null> {
    return await firstValueFrom(this.http.post<string | null>(CHECKOUT_URL, benefits));
  }
}

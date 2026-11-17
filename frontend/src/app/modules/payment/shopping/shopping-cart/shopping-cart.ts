import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  CheckoutBenefitsDto,
  CheckoutPreviewDto,
  GiftCardDto,
  OrderItemDto,
  PurchaseDto,
  ShoppingCartDto,
  WalletDto,
} from '../../api/payment-api-types';
import { Shopping } from '../shopping';

const GIFT_CARD_PAGE_SIZE = 20;

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-shopping-cart',
  styleUrl: './shopping-cart.scss',
  templateUrl: './shopping-cart.html',
})
export class ShoppingCart {
  protected readonly giftCardPageSize = GIFT_CARD_PAGE_SIZE;
  private readonly shopping = inject(Shopping);

  protected readonly cart = httpResource<ShoppingCartDto>(() => '/api/payment/cart');
  protected readonly wallet = httpResource<WalletDto>(() => '/api/payment/wallets/mine');
  protected readonly receivedPage = signal(1);
  protected readonly received = httpResource<PageResult<GiftCardDto>>(
    () => `/api/payment/gift-cards/received?page=${this.receivedPage()}&pageSize=${GIFT_CARD_PAGE_SIZE}`,
  );
  protected readonly validGiftCards = computed(
    () => this.received.value()?.items.filter((card) => card.status === 'Valid') ?? [],
  );

  protected readonly form = form(signal({ couponCode: '', giftCardId: '' }));
  protected readonly activeCouponCode = signal<string | null>(null);
  protected readonly purchaseId = signal<string | null>(null);
  protected readonly purchase = httpResource<PurchaseDto>(() => {
    const id = this.purchaseId();
    return id ? `/api/payment/purchases/${id}` : undefined;
  });
  protected readonly preview = httpResource<CheckoutPreviewDto>(() => {
    const current = this.cart.value();
    if (!current) {
      return undefined;
    }
    const params = new URLSearchParams();
    const code = this.activeCouponCode();
    const giftCardId = this.form.giftCardId().value();
    if (code) {
      params.set('couponCode', code);
    }
    if (giftCardId) {
      params.set('giftCardId', giftCardId);
    }
    const query = params.toString();
    return `/api/payment/cart/checkout-preview${query ? `?${query}` : ''}`;
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly paid = signal(false);

  protected applyCoupon(event: Event): void {
    event.preventDefault();
    const code = this.form.couponCode().value().trim();
    if (code) {
      this.activeCouponCode.set(code);
      this.error.set(null);
      this.paid.set(false);
    }
  }

  protected removeCoupon(): void {
    this.activeCouponCode.set(null);
    this.form().reset({ couponCode: '', giftCardId: this.form.giftCardId().value() });
    this.error.set(null);
    this.paid.set(false);
  }

  protected isLastGiftCardPage(page: number, totalCount: number): boolean {
    return page * GIFT_CARD_PAGE_SIZE >= totalCount;
  }

  protected previousGiftCards(): void {
    this.receivedPage.update((page) => page - 1);
  }

  protected nextGiftCards(): void {
    this.receivedPage.update((page) => page + 1);
  }

  protected localDate(date: string): string {
    return new Date(date).toLocaleDateString();
  }

  protected async remove(item: OrderItemDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.paid.set(false);
    try {
      await this.shopping.removeFromCart(item.entityId);
      this.cart.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, `${item.name} was not removed from the cart.`));
    } finally {
      this.pending.set(false);
    }
  }

  protected async checkout(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.paid.set(false);
    try {
      const couponCode = this.activeCouponCode();
      const giftCardId = this.form.giftCardId().value() || null;
      const benefits: CheckoutBenefitsDto | null = couponCode || giftCardId
        ? { couponCode, giftCardId }
        : null;
      const id = await this.shopping.checkout(benefits);
      this.purchaseId.set(id);
      this.paid.set(true);
      this.cart.reload();
      this.wallet.reload();
      this.received.reload();
      this.activeCouponCode.set(null);
      this.form().reset({ couponCode: '', giftCardId: '' });
    } catch (failure) {
      this.error.set(serverMessage(failure, 'The cart was not paid. Reload the page and try again.'));
    } finally {
      this.pending.set(false);
    }
  }
}

import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { OrderItemDto, ShoppingCartDto, WalletDto } from '../../api/payment-api-types';
import { Shopping } from '../shopping';

@Component({
  imports: [RouterLink],
  selector: 'app-shopping-cart',
  styleUrl: './shopping-cart.scss',
  templateUrl: './shopping-cart.html',
})
export class ShoppingCart {
  private readonly shopping = inject(Shopping);

  protected readonly cart = httpResource<ShoppingCartDto>(() => '/api/payment/cart');
  protected readonly wallet = httpResource<WalletDto>(() => '/api/payment/wallets/mine');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly paid = signal(false);

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
      await this.shopping.checkout();
      this.paid.set(true);
      this.cart.reload();
      this.wallet.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'The cart was not paid. Reload the page and try again.'));
    } finally {
      this.pending.set(false);
    }
  }
}

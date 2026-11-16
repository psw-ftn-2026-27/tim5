import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { serverMessage } from '../../../../shared/util/server-message';
import { OrderItemDto, ShoppingCartDto } from '../../api/payment-api-types';
import { Shopping } from '../shopping';

@Component({
  selector: 'app-shopping-cart',
  styleUrl: './shopping-cart.scss',
  templateUrl: './shopping-cart.html',
})
export class ShoppingCart {
  private readonly shopping = inject(Shopping);

  protected readonly cart = httpResource<ShoppingCartDto>(() => '/api/payment/cart');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async remove(item: OrderItemDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.shopping.removeFromCart(item.entityId);
      this.cart.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, `${item.name} was not removed from the cart.`));
    } finally {
      this.pending.set(false);
    }
  }
}

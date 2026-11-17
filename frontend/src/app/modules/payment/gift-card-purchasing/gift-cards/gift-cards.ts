import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, max, min, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { GiftCardDto } from '../../api/payment-api-types';
import { GiftCardPurchasing } from '../gift-card-purchasing';

const PAGE_SIZE = 20;

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-gift-cards',
  styleUrl: './gift-cards.scss',
  templateUrl: './gift-cards.html',
})
export class GiftCards {
  private readonly purchasing = inject(GiftCardPurchasing);

  protected readonly form = form(
    signal({ recipientUsername: '', amount: null as number | null, message: '' }),
    (path) => {
      required(path.recipientUsername, { message: 'Enter the recipient username.' });
      required(path.amount, { message: 'Enter an amount.' });
      min(path.amount, 10, { message: 'The amount must be at least 10 EC.' });
      max(path.amount, 1000, { message: 'The amount cannot exceed 1000 EC.' });
      validate(path.amount, ({ value }) =>
        value() !== null && !Number.isInteger(value())
          ? { kind: 'wholeNumber', message: 'The amount must be a whole number.' }
          : null,
      );
      validate(path.message, ({ value }) =>
        value().length > 200
          ? { kind: 'messageLength', message: 'The message cannot exceed 200 characters.' }
          : null,
      );
    },
  );

  protected readonly showForm = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly sentPage = signal(1);
  protected readonly receivedPage = signal(1);

  protected readonly sent = httpResource<PageResult<GiftCardDto>>(
    () => `/api/payment/gift-cards/sent?page=${this.sentPage()}&pageSize=${PAGE_SIZE}`,
  );
  protected readonly received = httpResource<PageResult<GiftCardDto>>(
    () => `/api/payment/gift-cards/received?page=${this.receivedPage()}&pageSize=${PAGE_SIZE}`,
  );

  protected async purchase(event: Event): Promise<void> {
    event.preventDefault();
    const values = this.form().value();
    if (values.amount === null || !this.form().valid()) {
      return;
    }

    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      await this.purchasing.purchase({
        recipientUsername: values.recipientUsername,
        amount: values.amount,
        message: values.message,
      });
      this.sentPage.set(1);
      this.sent.reload();
      this.form().reset({ recipientUsername: '', amount: null, message: '' });
      this.showForm.set(false);
      this.success.set(`Gift card of ${values.amount} EC sent to ${values.recipientUsername}.`);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'The gift card was not purchased.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected isLastPage(page: number, totalCount: number): boolean {
    return page * PAGE_SIZE >= totalCount;
  }

  protected previousSent(): void {
    this.sentPage.update((page) => page - 1);
  }

  protected nextSent(): void {
    this.sentPage.update((page) => page + 1);
  }

  protected previousReceived(): void {
    this.receivedPage.update((page) => page - 1);
  }

  protected nextReceived(): void {
    this.receivedPage.update((page) => page + 1);
  }

  protected localDate(date: string): string {
    return new Date(date).toLocaleDateString();
  }
}

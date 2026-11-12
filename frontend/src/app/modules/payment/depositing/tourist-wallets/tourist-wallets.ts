import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TouristBalanceDto } from '../../api/payment-api-types';
import { DepositForm } from '../deposit-form/deposit-form';
import { Depositing } from '../depositing';

@Component({
  imports: [DepositForm, FormField, RouterLink],
  selector: 'app-tourist-wallets',
  styleUrl: './tourist-wallets.scss',
  templateUrl: './tourist-wallets.html',
})
export class TouristWallets {
  private readonly depositing = inject(Depositing);

  protected readonly form = form(signal({ username: '' }), (path) => {
    required(path.username, { message: 'Enter part of a username.' });
  });

  private readonly searchedUsername = signal<string | null>(null);

  protected readonly tourists = httpResource<TouristBalanceDto[]>(() => {
    const username = this.searchedUsername();
    if (username === null) {
      return undefined;
    }
    return `/api/payment/tourists?username=${encodeURIComponent(username)}`;
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  protected search(event: Event): void {
    event.preventDefault();
    this.error.set(null);
    this.success.set(null);
    this.searchedUsername.set(this.form.username().value());
  }

  protected async deposit(tourist: TouristBalanceDto, amount: number): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      await this.depositing.deposit(tourist.touristId, { amount });
      this.tourists.reload();
      this.success.set(`Deposited ${amount} EC to ${tourist.username}.`);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'The deposit was not made.'));
    } finally {
      this.pending.set(false);
    }
  }
}

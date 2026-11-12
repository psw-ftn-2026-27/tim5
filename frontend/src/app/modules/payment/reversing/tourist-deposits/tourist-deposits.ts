import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TouristDepositDto, TouristDepositsDto } from '../../api/payment-api-types';
import { Reversing } from '../reversing';

@Component({
  imports: [RouterLink],
  selector: 'app-tourist-deposits',
  styleUrl: './tourist-deposits.scss',
  templateUrl: './tourist-deposits.html',
})
export class TouristDeposits {
  private readonly reversing = inject(Reversing);

  readonly touristId = input.required<string>();

  protected readonly tourist = httpResource<TouristDepositsDto>(
    () => `/api/payment/tourists/${this.touristId()}/deposits`,
  );

  protected readonly confirmingId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  protected localTime(time: string): string {
    return new Date(time).toLocaleString();
  }

  protected askToConfirm(deposit: TouristDepositDto): void {
    this.error.set(null);
    this.success.set(null);
    this.confirmingId.set(deposit.id);
  }

  protected cancel(): void {
    this.confirmingId.set(null);
  }

  protected async reverse(deposit: TouristDepositDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      await this.reversing.reverse(this.touristId(), deposit.id);
      this.confirmingId.set(null);
      this.tourist.reload();
      this.success.set(`Reversed the deposit of ${deposit.amount} EC.`);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'The deposit was not reversed.'));
    } finally {
      this.pending.set(false);
    }
  }
}

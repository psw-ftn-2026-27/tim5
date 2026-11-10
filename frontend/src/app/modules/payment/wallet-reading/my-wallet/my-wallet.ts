import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { WalletDto } from '../../api/payment-api-types';

@Component({
  selector: 'app-my-wallet',
  styleUrl: './my-wallet.scss',
  templateUrl: './my-wallet.html',
})
export class MyWallet {
  protected readonly wallet = httpResource<WalletDto>(() => '/api/payment/wallets/mine');

  protected localTime(occurredAt: string): string {
    return new Date(occurredAt).toLocaleString();
  }
}

import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PurchasedTourDto } from '../../api/payment-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-purchased-tours',
  styleUrl: './purchased-tours.scss',
  templateUrl: './purchased-tours.html',
})
export class PurchasedTours {
  protected readonly purchases = httpResource<PurchasedTourDto[]>(() => '/api/payment/purchases');

  protected localTime(purchasedAt: string): string {
    return new Date(purchasedAt).toLocaleString();
  }
}

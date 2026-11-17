import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateCouponDto, CreatedCouponDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/coupons';

@Injectable({ providedIn: 'root' })
export class CouponCreating {
  private readonly http = inject(HttpClient);

  async createCoupon(dto: CreateCouponDto): Promise<CreatedCouponDto> {
    return firstValueFrom(this.http.post<CreatedCouponDto>(BASE_URL, dto));
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/payment/tourists';

@Injectable({ providedIn: 'root' })
export class Reversing {
  private readonly http = inject(HttpClient);

  async reverse(touristId: string, depositId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${touristId}/deposits/${depositId}/reversal`, null),
    );
  }
}

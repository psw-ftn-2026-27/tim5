import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/exploration/executions';

@Injectable({ providedIn: 'root' })
export class TourExecution {
  private readonly http = inject(HttpClient);

  async abandon(): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/active/abandon`, null));
  }
}

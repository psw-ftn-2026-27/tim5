import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LocationDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/positions';

@Injectable({ providedIn: 'root' })
export class PositionSimulator {
  private readonly http = inject(HttpClient);

  async markPosition(dto: LocationDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/mine`, dto));
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LocationDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/executions';
const POSITIONS_URL = '/api/exploration/positions';

@Injectable({ providedIn: 'root' })
export class TourExecution {
  private readonly http = inject(HttpClient);

  // Served by the backend's PositionSimulator group, but run from the active tour page.
  async markPosition(dto: LocationDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${POSITIONS_URL}/mine`, dto));
  }

  // The server reads the explorer's position itself, so the check carries no body.
  async checkPosition(): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/active/position-check`, null));
  }

  async abandon(): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/active/abandon`, null));
  }
}

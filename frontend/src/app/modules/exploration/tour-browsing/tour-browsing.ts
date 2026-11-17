import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const EXECUTIONS_URL = '/api/exploration/executions';

@Injectable({ providedIn: 'root' })
export class TourBrowsing {
  private readonly http = inject(HttpClient);

  // Served by the backend's TourExecution group, but run from the tour preview page.
  async startTour(tourId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${EXECUTIONS_URL}/tours/${tourId}`, null));
  }
}

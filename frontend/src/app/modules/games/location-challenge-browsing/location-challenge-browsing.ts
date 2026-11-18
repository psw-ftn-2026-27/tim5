import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/games/location-challenges';

@Injectable({ providedIn: 'root' })
export class LocationChallengeBrowsing {
  private readonly http = inject(HttpClient);

  // Served by the backend's LocationChallengeActivation group, but run from the nearby challenges page.
  async activate(challengeId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${challengeId}/activation`, null));
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/games/location-challenges';

@Injectable({ providedIn: 'root' })
export class LocationChallengeActivation {
  private readonly http = inject(HttpClient);

  // The server reads the explorer's saved position itself, so the request has no body.
  async checkLocation(challengeId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${challengeId}/activation/location-check`, null),
    );
  }
}

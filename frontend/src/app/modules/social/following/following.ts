import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social/following';

@Injectable({ providedIn: 'root' })
export class Following {
  private readonly http = inject(HttpClient);

  async requestFollow(followeeId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${followeeId}`, {}));
  }

  async withdrawRequest(followeeId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${followeeId}/request`));
  }

  async unfollow(followeeId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${followeeId}`));
  }
}

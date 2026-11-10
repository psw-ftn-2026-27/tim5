import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/social/followers';

@Injectable({ providedIn: 'root' })
export class FollowerManagement {
  private readonly http = inject(HttpClient);

  async acceptRequest(followerId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${followerId}/accept`, {}));
  }

  async rejectRequest(followerId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${followerId}/reject`, {}));
  }

  async removeFollower(followerId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${followerId}`));
  }
}

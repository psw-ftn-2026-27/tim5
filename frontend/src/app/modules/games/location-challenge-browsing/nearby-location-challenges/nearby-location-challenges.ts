import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MapPin, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { NearbyLocationChallengesDto } from '../../api/games-api-types';
import { LocationChallengeBrowsing } from '../location-challenge-browsing';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-nearby-location-challenges',
  styleUrl: './nearby-location-challenges.scss',
  templateUrl: './nearby-location-challenges.html',
})
export class NearbyLocationChallenges {
  private readonly locationChallengeBrowsing = inject(LocationChallengeBrowsing);
  private readonly router = inject(Router);

  protected readonly nearby = httpResource<NearbyLocationChallengesDto>(
    () => '/api/games/location-challenges/nearby',
  );

  // The explorer's own position first, then the start location of every listed challenge.
  protected readonly pins = computed<MapPin[]>(() => {
    if (!this.nearby.hasValue()) {
      return [];
    }
    const { position, challenges } = this.nearby.value();
    const starts = challenges.map((challenge) => ({
      latitude: challenge.startLocation.latitude,
      longitude: challenge.startLocation.longitude,
      label: challenge.name,
    }));
    return position
      ? [{ latitude: position.latitude, longitude: position.longitude, label: 'You' }, ...starts]
      : starts;
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected distance(meters: number): string {
    return `${Math.round(meters)} m`;
  }

  protected async activate(challengeId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.locationChallengeBrowsing.activate(challengeId);
      await this.router.navigate(['/games/active', challengeId]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not activate the location challenge.'));
      this.nearby.reload();
    } finally {
      this.pending.set(false);
    }
  }
}

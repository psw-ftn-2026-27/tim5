import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MapCircle, MapPin, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { ActiveLocationChallengeDto } from '../../api/games-api-types';
import { LocationChallengeActivation } from '../location-challenge-activation';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-active-location-challenge',
  styleUrl: './active-location-challenge.scss',
  templateUrl: './active-location-challenge.html',
})
export class ActiveLocationChallenge {
  private readonly locationChallengeActivation = inject(LocationChallengeActivation);

  readonly id = input.required<string>();

  protected readonly challenge = httpResource<ActiveLocationChallengeDto>(
    () => `/api/games/location-challenges/${this.id()}/activation`,
  );

  // The hidden points lie somewhere inside this circle.
  protected readonly circle = computed<MapCircle | null>(() => {
    if (!this.challenge.hasValue()) {
      return null;
    }
    const { center, radiusInMeters } = this.challenge.value();
    return { latitude: center.latitude, longitude: center.longitude, radiusInMeters };
  });

  // Only a discovered point comes with its location; the label matches the number of its photo.
  protected readonly pins = computed<MapPin[]>(() => {
    if (!this.challenge.hasValue()) {
      return [];
    }
    const pins: MapPin[] = [];
    this.challenge.value().photos.forEach((photo, index) => {
      if (photo.location) {
        pins.push({
          latitude: photo.location.latitude,
          longitude: photo.location.longitude,
          label: `Point ${index + 1}`,
        });
      }
    });
    return pins;
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  protected async checkLocation(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      await this.locationChallengeActivation.checkLocation(this.id());
      this.success.set('You found a point!');
      this.challenge.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not check your location.'));
      // The challenge may have been completed meanwhile (for example in another tab).
      this.challenge.reload();
    } finally {
      this.pending.set(false);
    }
  }
}

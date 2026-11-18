import { httpResource } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MapCircle, MapView } from '../../../../shared/map/map';
import { ActiveLocationChallengeDto } from '../../api/games-api-types';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-active-location-challenge',
  styleUrl: './active-location-challenge.scss',
  templateUrl: './active-location-challenge.html',
})
export class ActiveLocationChallenge {
  readonly id = input.required<string>();

  protected readonly challenge = httpResource<ActiveLocationChallengeDto>(
    () => `/api/games/location-challenges/${this.id()}/activation`,
  );

  // The hidden points lie somewhere inside this circle; their locations never reach the client.
  protected readonly circle = computed<MapCircle | null>(() => {
    if (!this.challenge.hasValue()) {
      return null;
    }
    const { center, radiusInMeters } = this.challenge.value();
    return { latitude: center.latitude, longitude: center.longitude, radiusInMeters };
  });
}

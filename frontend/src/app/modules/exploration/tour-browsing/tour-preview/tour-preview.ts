import { httpResource } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MapPin, MapView } from '../../../../shared/map/map';
import { TourPreviewDto } from '../../api/exploration-api-types';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-tour-preview',
  styleUrl: './tour-preview.scss',
  templateUrl: './tour-preview.html',
})
export class TourPreview {
  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourPreviewDto>(
    () => `/api/exploration/tours/published/${this.id()}`,
  );

  // Only the public key points are shown, as pins without a path: a path through them
  // alone would not match the real tour.
  protected readonly pins = computed<MapPin[]>(() =>
    (this.tour.value()?.publicKeyPoints ?? []).map((keyPoint) => ({
      latitude: keyPoint.location.latitude,
      longitude: keyPoint.location.longitude,
      label: keyPoint.name,
    })),
  );
}

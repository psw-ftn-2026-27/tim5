import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MapPin, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourPreviewDto } from '../../api/exploration-api-types';
import { TourBrowsing } from '../tour-browsing';

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-tour-preview',
  styleUrl: './tour-preview.scss',
  templateUrl: './tour-preview.html',
})
export class TourPreview {
  private readonly tourBrowsing = inject(TourBrowsing);
  private readonly router = inject(Router);

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

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  // A 404 from starting means the explorer has not set a position yet, so the page links to the simulator.
  protected readonly needsPosition = signal(false);

  protected async start(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.needsPosition.set(false);
    try {
      await this.tourBrowsing.startTour(tourId);
      await this.router.navigate(['/exploration/execution']);
    } catch (failure) {
      this.needsPosition.set(failure instanceof HttpErrorResponse && failure.status === 404);
      this.error.set(serverMessage(failure, 'Could not start the tour.'));
    } finally {
      this.pending.set(false);
    }
  }
}

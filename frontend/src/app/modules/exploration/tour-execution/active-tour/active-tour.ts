import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MapPin, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { ActiveTourExecutionDto } from '../../api/exploration-api-types';
import { TourExecution } from '../tour-execution';

const CHECK_INTERVAL_MS = 10_000;

@Component({
  imports: [MapView, RouterLink],
  selector: 'app-active-tour',
  styleUrl: './active-tour.scss',
  templateUrl: './active-tour.html',
})
export class ActiveTour {
  private readonly tourExecution = inject(TourExecution);
  private readonly router = inject(Router);

  protected readonly execution = httpResource<ActiveTourExecutionDto>(
    () => '/api/exploration/executions/active',
  );

  // 404 means the explorer has no active tour; that is an empty state, not an error.
  protected readonly hasNoActiveTour = computed(() => {
    const failure = this.execution.error();
    return failure instanceof HttpErrorResponse && failure.status === 404;
  });

  // The tour from the last successful load. It stays known after the tour ends, so the page
  // can still link back to it and navigate there when the next check gets a 404.
  protected readonly knownTour = signal<{ id: string; name: string } | null>(null);

  // The map refits and recomputes the route whenever it gets a new pins array. The pins are
  // built from plain values, so the reload every 10 seconds makes a new array only when the
  // explorer or the next key point has really moved.
  private readonly youLatitude = computed(() =>
    this.execution.hasValue() ? this.execution.value().currentPosition.latitude : null,
  );
  private readonly youLongitude = computed(() =>
    this.execution.hasValue() ? this.execution.value().currentPosition.longitude : null,
  );
  private readonly nextName = computed(() =>
    this.execution.hasValue() ? this.execution.value().nextKeyPoint.name : null,
  );
  private readonly nextLatitude = computed(() =>
    this.execution.hasValue() ? this.execution.value().nextKeyPoint.location.latitude : null,
  );
  private readonly nextLongitude = computed(() =>
    this.execution.hasValue() ? this.execution.value().nextKeyPoint.location.longitude : null,
  );

  // The path goes from the explorer's position to the next key point. "You" stays in the pins
  // even though the map is in picking mode, because the route is drawn only between pins.
  protected readonly pins = computed<MapPin[]>(() => {
    const youLatitude = this.youLatitude();
    const youLongitude = this.youLongitude();
    const nextName = this.nextName();
    const nextLatitude = this.nextLatitude();
    const nextLongitude = this.nextLongitude();
    if (
      youLatitude === null ||
      youLongitude === null ||
      nextName === null ||
      nextLatitude === null ||
      nextLongitude === null
    ) {
      return [];
    }
    return [
      { latitude: youLatitude, longitude: youLongitude, label: 'You' },
      { latitude: nextLatitude, longitude: nextLongitude, label: nextName },
    ];
  });

  // The unlocked secret is the key point completed last; the server orders the list by time.
  protected readonly unlocked = computed(() => {
    if (!this.execution.hasValue()) {
      return null;
    }
    return this.execution.value().completedKeyPoints.at(-1) ?? null;
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  private readonly timer = setInterval(() => this.checkPosition(), CHECK_INTERVAL_MS);

  constructor() {
    inject(DestroyRef).onDestroy(() => clearInterval(this.timer));
  }

  protected async markPosition(location: MapPin): Promise<void> {
    this.rememberTour();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourExecution.markPosition({
        latitude: location.latitude,
        longitude: location.longitude,
      });
      this.execution.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save your position.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async abandon(tourId: string): Promise<void> {
    this.rememberTour();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourExecution.abandon();
      await this.router.navigate(['/exploration', tourId]);
    } catch (failure) {
      // A 404 means the tour is no longer active; the reload then shows that state.
      if (!(failure instanceof HttpErrorResponse && failure.status === 404)) {
        this.error.set(serverMessage(failure, 'Could not abandon the tour.'));
      }
      this.execution.reload();
    } finally {
      this.pending.set(false);
    }
  }

  private async checkPosition(): Promise<void> {
    this.rememberTour();
    const tour = this.knownTour();
    if (tour === null) {
      // The page was opened without an active tour, so there is nothing to check.
      if (!this.execution.isLoading()) {
        clearInterval(this.timer);
      }
      return;
    }
    if (this.pending()) {
      return;
    }
    this.error.set(null);
    try {
      await this.tourExecution.checkPosition();
      this.execution.reload();
    } catch (failure) {
      // A 404 means the session is gone (completed, or abandoned in another tab), so the
      // explorer moves to the tour preview without a message.
      if (failure instanceof HttpErrorResponse && failure.status === 404) {
        await this.router.navigate(['/exploration', tour.id]);
        return;
      }
      this.error.set(serverMessage(failure, 'Could not check your position.'));
    }
  }

  // Every reload follows a command, so remembering the tour before each command keeps the
  // tour from the last successful load.
  private rememberTour(): void {
    if (this.execution.hasValue()) {
      const current = this.execution.value();
      this.knownTour.set({ id: current.tourId, name: current.tourName });
    }
  }

  protected localTime(reachedAt: string): string {
    return new Date(reachedAt).toLocaleString();
  }
}

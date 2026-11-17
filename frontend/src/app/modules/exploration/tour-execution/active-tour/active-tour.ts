import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MapPin, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { ActiveTourExecutionDto } from '../../api/exploration-api-types';
import { TourExecution } from '../tour-execution';

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

  // The path goes from the explorer's position to the next key point.
  protected readonly pins = computed<MapPin[]>(() => {
    if (!this.execution.hasValue()) {
      return [];
    }
    const current = this.execution.value();
    return [
      {
        latitude: current.currentPosition.latitude,
        longitude: current.currentPosition.longitude,
        label: 'You',
      },
      {
        latitude: current.nextKeyPoint.location.latitude,
        longitude: current.nextKeyPoint.location.longitude,
        label: current.nextKeyPoint.name,
      },
    ];
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async abandon(tourId: string): Promise<void> {
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
}

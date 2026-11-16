import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { MapPin, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { TouristPositionDto } from '../../api/exploration-api-types';
import { PositionSimulator } from '../position-simulator';

@Component({
  imports: [MapView],
  selector: 'app-simulator',
  styleUrl: './simulator.scss',
  templateUrl: './simulator.html',
})
export class Simulator {
  private readonly positionSimulator = inject(PositionSimulator);

  protected readonly position = httpResource<TouristPositionDto>(
    () => '/api/exploration/positions/mine',
  );

  // 404 means the user has not marked a position yet; that is an empty state, not an error.
  protected readonly hasNoPosition = computed(() => {
    const failure = this.position.error();
    return failure instanceof HttpErrorResponse && failure.status === 404;
  });

  // The pin is always the position the server holds, so it moves only after a confirmed save.
  protected readonly savedLocation = computed<MapPin | null>(() => {
    if (!this.position.hasValue()) {
      return null;
    }
    const current = this.position.value();
    return { latitude: current.latitude, longitude: current.longitude };
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async mark(location: MapPin): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.positionSimulator.markPosition({
        latitude: location.latitude,
        longitude: location.longitude,
      });
      this.position.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Server ne odgovara.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected localTime(markedAt: string): string {
    return new Date(markedAt).toLocaleString();
  }
}

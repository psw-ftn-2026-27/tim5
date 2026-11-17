import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { MapPin, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeAuthoring } from '../location-challenge-authoring';

@Component({
  imports: [FormField, MapView, RouterLink],
  selector: 'app-create-location-challenge',
  styleUrl: './create-location-challenge.scss',
  templateUrl: './create-location-challenge.html',
})
export class CreateLocationChallenge {
  private readonly challengeAuthoring = inject(LocationChallengeAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({
      name: '',
      xp: null as number | null,
    }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.xp, { message: 'XP is required.' });
      min(path.xp, 1, { message: 'XP must be greater than 0.' });
    },
  );

  // The start location is picked on the map, so it lives next to the form, not in it.
  protected readonly startLocation = signal<MapPin | null>(null);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly canSave = computed(
    () => this.form().valid() && this.startLocation() !== null && !this.pending(),
  );

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    const xp = this.form.xp().value();
    const startLocation = this.startLocation();
    if (xp === null || startLocation === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      const challenge = await this.challengeAuthoring.create({
        name: this.form.name().value(),
        xp,
        startLocation: { latitude: startLocation.latitude, longitude: startLocation.longitude },
      });
      await this.router.navigate(['/games/location-challenges', challenge.id]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the location challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}

import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { ImagePicker, PickedImage } from '../../../../shared/image-picker/image-picker';
import { MapPin, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { HiddenPointDto, LocationChallengeDto } from '../../api/games-api-types';
import { LocationChallengeAuthoring } from '../location-challenge-authoring';

@Component({
  imports: [FormField, ImagePicker, MapView, RouterLink],
  selector: 'app-location-challenge-details',
  styleUrl: './location-challenge-details.scss',
  templateUrl: './location-challenge-details.html',
})
export class LocationChallengeDetails {
  private readonly challengeAuthoring = inject(LocationChallengeAuthoring);

  readonly id = input.required<string>();

  protected readonly challenge = httpResource<LocationChallengeDto>(
    () => `/api/games/location-challenges/${this.id()}`,
  );

  protected readonly startPin = computed<MapPin | null>(() => {
    if (!this.challenge.hasValue()) {
      return null;
    }
    const start = this.challenge.value().startLocation;
    return { latitude: start.latitude, longitude: start.longitude, label: 'Start' };
  });

  protected readonly hiddenPointPins = computed<MapPin[]>(() => {
    if (!this.challenge.hasValue()) {
      return [];
    }
    return this.challenge.value().hiddenPoints.map((point, index) => ({
      latitude: point.location.latitude,
      longitude: point.location.longitude,
      label: `Point ${index + 1}`,
    }));
  });

  // The main map shows the start location and every saved hidden point.
  protected readonly pins = computed<MapPin[]>(() => {
    const start = this.startPin();
    return start ? [start, ...this.hiddenPointPins()] : this.hiddenPointPins();
  });

  // One command runs at a time; its server message is shown above the hidden points.
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The basic data form: name and XP in the form, the start location picked on its map.
  protected readonly editingChallenge = signal(false);
  protected readonly challengeForm = form(
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
  protected readonly startLocation = signal<MapPin | null>(null);
  protected readonly canSaveChallenge = computed(
    () => this.challengeForm().valid() && this.startLocation() !== null && !this.pending(),
  );

  // The hidden point form adds a point, or changes the point whose id it holds.
  protected readonly editingPointId = signal<string | null>(null);
  // The picked location is also handed back to the map as its initial location, so
  // clearing the form also clears the picked dot on the map.
  protected readonly pointLocation = signal<MapPin | null>(null);
  // While a point is being changed, its own pin is left out: the picked dot stands for it.
  protected readonly pointFormPins = computed<MapPin[]>(() => {
    const start = this.startPin();
    const editingPointId = this.editingPointId();
    const points = this.challenge.hasValue() ? this.challenge.value().hiddenPoints : [];
    const others = this.hiddenPointPins().filter((_, index) => points[index]?.id !== editingPointId);
    return start ? [start, ...others] : others;
  });

  // The form holds a single image. While it uploads its key is a temporary one; once the
  // server accepts it, the key becomes the image id the server returned.
  protected readonly image = signal<PickedImage | null>(null);
  protected readonly images = computed(() => {
    const image = this.image();
    return image ? [image] : [];
  });
  protected readonly imageError = signal<string | null>(null);
  protected readonly canSavePoint = computed(
    () =>
      this.image() !== null &&
      !this.image()?.uploading &&
      this.pointLocation() !== null &&
      !this.pending(),
  );

  protected startEditChallenge(challenge: LocationChallengeDto): void {
    this.error.set(null);
    this.challengeForm().reset({ name: challenge.name, xp: challenge.xp });
    this.startLocation.set({
      latitude: challenge.startLocation.latitude,
      longitude: challenge.startLocation.longitude,
    });
    this.editingChallenge.set(true);
  }

  protected closeEditChallenge(): void {
    this.editingChallenge.set(false);
  }

  protected async saveChallenge(event: Event): Promise<void> {
    event.preventDefault();
    const xp = this.challengeForm.xp().value();
    const startLocation = this.startLocation();
    if (xp === null || startLocation === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.challengeAuthoring.update(this.id(), {
        name: this.challengeForm.name().value(),
        xp,
        startLocation: { latitude: startLocation.latitude, longitude: startLocation.longitude },
      });
      this.editingChallenge.set(false);
      this.challenge.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not change the location challenge.'));
      this.challenge.reload();
    } finally {
      this.pending.set(false);
    }
  }

  protected async upload(file: File): Promise<void> {
    this.imageError.set(null);
    const previewUrl = URL.createObjectURL(file);
    this.image.set({ key: 'uploading', previewUrl, uploading: true });
    try {
      const uploaded = await this.challengeAuthoring.uploadImage(file);
      this.image.set({ key: uploaded.id, previewUrl: uploaded.url, uploading: false });
    } catch (failure) {
      this.image.set(null);
      this.imageError.set(serverMessage(failure, 'Could not upload the image.'));
    } finally {
      URL.revokeObjectURL(previewUrl);
    }
  }

  // There is no endpoint for deleting an image; it is only taken out of the form.
  protected removeImage(): void {
    this.imageError.set(null);
    this.image.set(null);
  }

  protected startEditPoint(point: HiddenPointDto): void {
    this.error.set(null);
    this.editingPointId.set(point.id);
    this.pointLocation.set({ latitude: point.location.latitude, longitude: point.location.longitude });
    this.imageError.set(null);
    this.image.set({ key: point.imageId, previewUrl: point.imageUrl, uploading: false });
  }

  protected clearPointForm(): void {
    this.editingPointId.set(null);
    this.pointLocation.set(null);
    this.image.set(null);
    this.imageError.set(null);
  }

  protected async savePoint(event: Event): Promise<void> {
    event.preventDefault();
    const image = this.image();
    const location = this.pointLocation();
    if (image === null || location === null) {
      return;
    }
    const dto = {
      imageId: image.key,
      location: { latitude: location.latitude, longitude: location.longitude },
    };
    const pointId = this.editingPointId();

    this.pending.set(true);
    this.error.set(null);
    try {
      if (pointId === null) {
        await this.challengeAuthoring.addHiddenPoint(this.id(), dto);
      } else {
        await this.challengeAuthoring.changeHiddenPoint(this.id(), pointId, dto);
      }
      this.clearPointForm();
      this.challenge.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the hidden point.'));
      this.challenge.reload();
    } finally {
      this.pending.set(false);
    }
  }

  protected async removePoint(pointId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.challengeAuthoring.removeHiddenPoint(this.id(), pointId);
      this.challenge.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not remove the hidden point.'));
      this.challenge.reload();
    } finally {
      this.pending.set(false);
    }
  }
}

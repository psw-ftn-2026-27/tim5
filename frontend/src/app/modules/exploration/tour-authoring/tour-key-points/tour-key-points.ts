import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, max, min, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { ImagePicker, PickedImage } from '../../../../shared/image-picker/image-picker';
import { MapPin, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { KeyPointDto, TourWithKeyPointsDto } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

const EMPTY_FORM = {
  name: '',
  description: '',
  secret: '',
  isPublic: false,
  latitude: null as number | null,
  longitude: null as number | null,
};

@Component({
  imports: [FormField, ImagePicker, MapView, RouterLink],
  selector: 'app-tour-key-points',
  styleUrl: './tour-key-points.scss',
  templateUrl: './tour-key-points.html',
})
export class TourKeyPoints {
  private readonly tourAuthoring = inject(TourAuthoring);

  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourWithKeyPointsDto>(
    () => `/api/exploration/tours/mine/${this.id()}`,
  );

  // The main map shows only the saved key points, in their order.
  protected readonly pins = computed<MapPin[]>(() =>
    (this.tour.value()?.keyPoints ?? []).map((keyPoint) => ({
      latitude: keyPoint.location.latitude,
      longitude: keyPoint.location.longitude,
      label: `${keyPoint.order}. ${keyPoint.name}`,
    })),
  );

  // The length the main map reported last; null means it is unknown. The map does not
  // report a failed route, so the page clears the length itself whenever it changes the pins.
  protected readonly lengthInKm = signal<number | null>(null);
  protected readonly lengthText = computed(() => this.lengthInKm()?.toFixed(2));

  // At most one form is open: the "Add key point" form, or the edit row of one key point.
  protected readonly editingKeyPointId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = form(signal({ ...EMPTY_FORM }), (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
    required(path.latitude, { message: 'Latitude is required.' });
    min(path.latitude, -90, { message: 'Latitude must be between -90 and 90.' });
    max(path.latitude, 90, { message: 'Latitude must be between -90 and 90.' });
    required(path.longitude, { message: 'Longitude is required.' });
    min(path.longitude, -180, { message: 'Longitude must be between -180 and 180.' });
    max(path.longitude, 180, { message: 'Longitude must be between -180 and 180.' });
  });

  // The form's own small map shows one pin, once both coordinates are filled in and in range.
  protected readonly formPins = computed<MapPin[]>(() => {
    const latitude = this.form.latitude().value();
    const longitude = this.form.longitude().value();
    if (latitude === null || longitude === null) {
      return [];
    }
    if (!this.form.latitude().valid() || !this.form.longitude().valid()) {
      return [];
    }
    return [{ latitude, longitude }];
  });

  // The form holds a single image. While it uploads its key is a temporary one; once the
  // server accepts it, the key becomes the image id the server returned.
  protected readonly image = signal<PickedImage | null>(null);
  protected readonly images = computed(() => {
    const image = this.image();
    return image ? [image] : [];
  });
  protected readonly imageError = signal<string | null>(null);
  protected readonly canSave = computed(
    () =>
      this.form().valid() && this.image() !== null && !this.image()?.uploading && !this.pending(),
  );

  protected async upload(file: File): Promise<void> {
    this.imageError.set(null);
    const previewUrl = URL.createObjectURL(file);
    this.image.set({ key: 'uploading', previewUrl, uploading: true });
    try {
      const uploaded = await this.tourAuthoring.uploadImage(file);
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

  protected startEdit(keyPoint: KeyPointDto): void {
    this.error.set(null);
    this.editingKeyPointId.set(keyPoint.id);
    this.form().reset({
      name: keyPoint.name,
      description: keyPoint.description,
      secret: keyPoint.secret ?? '',
      isPublic: keyPoint.isPublic,
      latitude: keyPoint.location.latitude,
      longitude: keyPoint.location.longitude,
    });
    this.imageError.set(null);
    this.image.set({ key: keyPoint.imageId, previewUrl: keyPoint.imageUrl, uploading: false });
  }

  protected closeEdit(): void {
    this.editingKeyPointId.set(null);
    this.clearForm();
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    const image = this.image();
    const latitude = this.form.latitude().value();
    const longitude = this.form.longitude().value();
    if (image === null || latitude === null || longitude === null) {
      return;
    }
    const secret = this.form.secret().value().trim();
    const dto = {
      name: this.form.name().value(),
      description: this.form.description().value(),
      imageId: image.key,
      secret: secret === '' ? null : secret,
      isPublic: this.form.isPublic().value(),
      location: { latitude, longitude },
    };
    const keyPointId = this.editingKeyPointId();

    this.pending.set(true);
    this.error.set(null);
    try {
      if (keyPointId === null) {
        await this.tourAuthoring.addKeyPoint(this.id(), dto);
      } else {
        await this.tourAuthoring.changeKeyPoint(this.id(), keyPointId, dto);
      }
      this.closeEdit();
      this.reloadKeyPoints();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the key point.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(keyPointId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.removeKeyPoint(this.id(), keyPointId);
      this.reloadKeyPoints();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not remove the key point.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async recordLength(lengthInKm: number): Promise<void> {
    this.lengthInKm.set(lengthInKm);
    if (this.tour.value()?.status !== 'Draft') {
      return;
    }
    // No reload here: a reload hands the map new pins, the map measures the route again
    // and reports a length again, which would send RecordLength again in an endless loop.
    try {
      await this.tourAuthoring.recordLength(this.id(), { lengthInKm });
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not record the tour length.'));
    }
  }

  private reloadKeyPoints(): void {
    // The pins are about to change, so the old length no longer holds.
    this.lengthInKm.set(null);
    this.tour.reload();
  }

  private clearForm(): void {
    this.form().reset({ ...EMPTY_FORM });
    this.image.set(null);
    this.imageError.set(null);
  }
}

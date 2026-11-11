import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { ImagePicker, PickedImage } from '../../../../shared/image-picker/image-picker';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubAuthoring } from '../club-authoring';

@Component({
  imports: [FormField, ImagePicker],
  selector: 'app-create-club',
  styleUrl: './create-club.scss',
  templateUrl: './create-club.html',
})
export class CreateClub {
  private readonly clubAuthoring = inject(ClubAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(signal({ name: '', description: '' }), (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // While an image is uploading its key is a temporary one; once the server accepts it,
  // the key becomes the image id the server returned.
  protected readonly images = signal<PickedImage[]>([]);
  protected readonly imageError = signal<string | null>(null);
  protected readonly uploading = computed(() => this.images().some((image) => image.uploading));

  private nextTemporaryKey = 0;

  protected async upload(file: File): Promise<void> {
    this.imageError.set(null);
    const picked: PickedImage = {
      key: `uploading-${this.nextTemporaryKey++}`,
      previewUrl: URL.createObjectURL(file),
      uploading: true,
    };
    this.images.update((images) => [...images, picked]);

    try {
      const uploaded = await this.clubAuthoring.uploadImage(file);
      this.images.update((images) =>
        images.map((image) =>
          image.key === picked.key ? { ...image, key: uploaded.id, uploading: false } : image,
        ),
      );
    } catch (failure) {
      this.images.update((images) => images.filter((image) => image.key !== picked.key));
      URL.revokeObjectURL(picked.previewUrl);
      this.imageError.set(serverMessage(failure, 'Could not upload the image.'));
    }
  }

  protected async remove(removed: PickedImage): Promise<void> {
    this.imageError.set(null);
    const position = this.images().indexOf(removed);
    this.images.update((images) => images.filter((image) => image.key !== removed.key));

    try {
      await this.clubAuthoring.deleteImage(removed.key);
      URL.revokeObjectURL(removed.previewUrl);
    } catch (failure) {
      // Put the thumbnail back where it was.
      this.images.update((images) => [
        ...images.slice(0, position),
        removed,
        ...images.slice(position),
      ]);
      this.imageError.set(serverMessage(failure, 'Could not remove the image.'));
    }
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubAuthoring.create({
        name: this.form.name().value(),
        description: this.form.description().value(),
        imageIds: this.images().map((image) => image.key),
      });
      await this.router.navigate(['/social/clubs']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the club.'));
    } finally {
      this.pending.set(false);
    }
  }
}

import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { ImagePicker, PickedImage } from '../../../../shared/image-picker/image-picker';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto } from '../../api/social-api-types';
import { ClubReading } from '../club-reading';

@Component({
  imports: [FormField, ImagePicker, RouterLink],
  selector: 'app-club-detail',
  styleUrl: './club-detail.scss',
  templateUrl: './club-detail.html',
})
export class ClubDetail {
  private readonly clubReading = inject(ClubReading);
  private readonly router = inject(Router);
  private readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<ClubDto>(() => `/api/social/clubs/${this.id()}`);

  protected readonly isOwner = computed(
    () => this.detail.value()?.ownerId === this.auth.user()?.id,
  );

  protected readonly editing = signal(false);
  protected readonly confirmingDelete = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = form(signal({ name: '', description: '' }), (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  // While an image is uploading its key is a temporary one; once the server accepts it,
  // the key becomes the image id the server returned.
  protected readonly images = signal<PickedImage[]>([]);
  protected readonly imageError = signal<string | null>(null);
  protected readonly uploading = computed(() => this.images().some((image) => image.uploading));

  // The club's images are already linked on the server, so removing one only drops it
  // from the list; only images uploaded during this edit are deleted on the server.
  private clubImageIds: string[] = [];
  private nextTemporaryKey = 0;

  protected startEdit(club: ClubDto): void {
    this.error.set(null);
    this.confirmingDelete.set(false);
    this.form().reset({ name: club.name, description: club.description });
    this.clubImageIds = club.images.map((image) => image.id);
    this.images.set(
      club.images.map((image) => ({ key: image.id, previewUrl: image.url, uploading: false })),
    );
    this.imageError.set(null);
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.editing.set(false);
  }

  protected async upload(file: File): Promise<void> {
    this.imageError.set(null);
    const picked: PickedImage = {
      key: `uploading-${this.nextTemporaryKey++}`,
      previewUrl: URL.createObjectURL(file),
      uploading: true,
    };
    this.images.update((images) => [...images, picked]);

    try {
      const uploaded = await this.clubReading.uploadImage(file);
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
    if (this.clubImageIds.includes(removed.key)) {
      return;
    }

    try {
      await this.clubReading.deleteImage(removed.key);
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

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubReading.update(this.id(), {
        name: this.form.name().value(),
        description: this.form.description().value(),
        imageIds: this.images().map((image) => image.key),
      });
      this.detail.reload();
      this.editing.set(false);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not update the club.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected startDelete(): void {
    this.error.set(null);
    this.editing.set(false);
    this.confirmingDelete.set(true);
  }

  protected cancelDelete(): void {
    this.confirmingDelete.set(false);
  }

  protected async confirmDelete(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubReading.delete(this.id());
      await this.router.navigate(['/social/clubs']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the club.'));
    } finally {
      this.pending.set(false);
    }
  }
}

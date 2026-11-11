import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto } from '../../api/social-api-types';
import { ClubReading } from '../club-reading';

@Component({
  imports: [FormField, RouterLink],
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

  protected readonly form = form(signal({ name: '', description: '', images: '' }), (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
    required(path.images, { message: 'At least one image URL is required.' });
  });

  protected startEdit(club: ClubDto): void {
    this.error.set(null);
    this.confirmingDelete.set(false);
    this.form().reset({
      name: club.name,
      description: club.description,
      images: club.images.join('\n'),
    });
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.editing.set(false);
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubReading.update(this.id(), {
        name: this.form.name().value(),
        description: this.form.description().value(),
        images: this.form
          .images()
          .value()
          .split('\n')
          .map((url) => url.trim())
          .filter((url) => url !== ''),
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

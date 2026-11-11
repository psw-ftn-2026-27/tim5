import { Component, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubAuthoring } from '../club-authoring';

@Component({
  imports: [FormField],
  selector: 'app-create-club',
  styleUrl: './create-club.scss',
  templateUrl: './create-club.html',
})
export class CreateClub {
  private readonly clubAuthoring = inject(ClubAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(signal({ name: '', description: '', images: '' }), (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
    required(path.images, { message: 'At least one image URL is required.' });
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubAuthoring.create({
        name: this.form.name().value(),
        description: this.form.description().value(),
        images: this.form
          .images()
          .value()
          .split('\n')
          .map((url) => url.trim())
          .filter((url) => url !== ''),
      });
      await this.router.navigate(['/social/clubs']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the club.'));
    } finally {
      this.pending.set(false);
    }
  }
}

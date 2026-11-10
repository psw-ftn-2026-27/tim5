import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { UserDto } from '../../api/social-api-types';
import { Following } from '../following';

@Component({
  imports: [FormField],
  selector: 'app-user-search',
  styleUrl: './user-search.scss',
  templateUrl: './user-search.html',
})
export class UserSearch {
  private readonly following = inject(Following);
  private readonly router = inject(Router);

  protected readonly form = form(signal({ username: '' }), (path) => {
    required(path.username, { message: 'Enter part of a username.' });
  });

  private readonly searchedUsername = signal<string | null>(null);

  protected readonly users = httpResource<UserDto[]>(() => {
    const username = this.searchedUsername();
    if (username === null) {
      return undefined;
    }
    return `/api/social/following/users?username=${encodeURIComponent(username)}`;
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected search(event: Event): void {
    event.preventDefault();
    this.error.set(null);
    this.searchedUsername.set(this.form.username().value());
  }

  protected async follow(userId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.following.requestFollow(userId);
      await this.router.navigate(['/social/following']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Follow request was not sent.'));
    } finally {
      this.pending.set(false);
    }
  }
}

import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { serverMessage } from '../../../../shared/util/server-message';
import { FollowingDto } from '../../api/social-api-types';
import { Following } from '../following';

@Component({
  selector: 'app-following-list',
  styleUrl: './following-list.scss',
  templateUrl: './following-list.html',
})
export class FollowingList {
  private readonly following = inject(Following);

  protected readonly followings = httpResource<FollowingDto[]>(() => '/api/social/following');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async withdraw(userId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.following.withdrawRequest(userId);
      this.followings.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Request was not withdrawn.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async unfollow(userId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.following.unfollow(userId);
      this.followings.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not unfollow.'));
    } finally {
      this.pending.set(false);
    }
  }
}

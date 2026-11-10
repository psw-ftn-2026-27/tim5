import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { serverMessage } from '../../../../shared/util/server-message';
import { FollowersDto } from '../../api/social-api-types';
import { FollowerManagement } from '../follower-management';

@Component({
  selector: 'app-follower-list',
  styleUrl: './follower-list.scss',
  templateUrl: './follower-list.html',
})
export class FollowerList {
  private readonly followerManagement = inject(FollowerManagement);

  protected readonly followers = httpResource<FollowersDto>(() => '/api/social/followers');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async accept(userId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followerManagement.acceptRequest(userId);
      this.followers.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Request was not accepted.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async reject(userId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followerManagement.rejectRequest(userId);
      this.followers.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Request was not rejected.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(userId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followerManagement.removeFollower(userId);
      this.followers.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Follower was not removed.'));
    } finally {
      this.pending.set(false);
    }
  }
}

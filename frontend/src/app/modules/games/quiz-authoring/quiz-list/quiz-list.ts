import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizSummaryDto } from '../../api/games-api-types';
import { QuizAuthoring } from '../quiz-authoring';

@Component({
  imports: [RouterLink],
  selector: 'app-quiz-list',
  styleUrl: './quiz-list.scss',
  templateUrl: './quiz-list.html',
})
export class QuizList {
  private readonly quizAuthoring = inject(QuizAuthoring);

  protected readonly quizzes = httpResource<QuizSummaryDto[]>(() => '/api/games/quizzes');

  protected readonly actionPending = signal(false);
  protected readonly actionError = signal<string | null>(null);

  protected async delete(quizId: string): Promise<void> {
    this.actionPending.set(true);
    this.actionError.set(null);
    try {
      await this.quizAuthoring.delete(quizId);
      this.quizzes.reload();
    } catch (failure) {
      this.actionError.set(serverMessage(failure, 'Could not delete the quiz.'));
      this.quizzes.reload();
    } finally {
      this.actionPending.set(false);
    }
  }
}

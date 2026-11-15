import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, max, min, required, validate } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { MapPin, MapView } from '../../../../shared/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuestionDto, QuizDto } from '../../api/games-api-types';
import { QuizAuthoring } from '../quiz-authoring';

@Component({
  imports: [FormField, MapView, RouterLink],
  selector: 'app-edit-quiz',
  styleUrl: './edit-quiz.scss',
  templateUrl: './edit-quiz.html',
})
export class EditQuiz {
  private readonly quizAuthoring = inject(QuizAuthoring);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly quiz = httpResource<QuizDto>(() => `/api/games/quizzes/${this.id()}`);

  protected readonly editing = signal(false);

  private readonly model = signal({
    name: '',
    instructions: '',
    xp: null as number | null,
    latitude: null as number | null,
    longitude: null as number | null,
    questions: [] as QuestionDto[],
  });

  protected readonly form = form(this.model, (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.instructions, { message: 'Instructions are required.' });
    required(path.xp, { message: 'XP is required.' });
    min(path.xp, 1, { message: 'XP must be greater than 0.' });
    required(path.latitude, { message: 'Latitude is required.' });
    min(path.latitude, -90, { message: 'Latitude must be between -90 and 90.' });
    max(path.latitude, 90, { message: 'Latitude must be between -90 and 90.' });
    required(path.longitude, { message: 'Longitude is required.' });
    min(path.longitude, -180, { message: 'Longitude must be between -180 and 180.' });
    max(path.longitude, 180, { message: 'Longitude must be between -180 and 180.' });
    validate(path.questions, ({ value }) => {
      const questions = value();
      if (questions.length === 0) {
        return { kind: 'noQuestions', message: 'Add at least one question.' };
      }
      for (const [index, question] of questions.entries()) {
        const number = index + 1;
        if (question.text.trim() === '') {
          return { kind: 'questionText', message: `Question ${number} needs a text.` };
        }
        if (question.answers.length < 2) {
          return { kind: 'tooFewAnswers', message: `Question ${number} needs at least two answers.` };
        }
        if (question.answers.some((answer) => answer.text.trim() === '')) {
          return { kind: 'answerText', message: `Every answer of question ${number} needs a text.` };
        }
        if (question.answers.filter((answer) => answer.isCorrect).length !== 1) {
          return { kind: 'correctAnswer', message: `Question ${number} needs exactly one correct answer.` };
        }
      }
      return null;
    });
  });

  // The map only shows a pin once both coordinates are filled in and in range.
  protected readonly pins = computed<MapPin[]>(() => {
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

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected startEdit(quiz: QuizDto): void {
    this.error.set(null);
    this.form().reset({
      name: quiz.name,
      instructions: quiz.instructions,
      xp: quiz.xp,
      latitude: quiz.location.latitude,
      longitude: quiz.location.longitude,
      questions: quiz.questions.map((question) => ({
        text: question.text,
        answers: question.answers.map((answer) => ({ ...answer })),
      })),
    });
    this.editing.set(true);
  }

  protected addQuestion(): void {
    this.model.update((model) => ({
      ...model,
      questions: [
        ...model.questions,
        {
          text: '',
          answers: [
            { text: '', isCorrect: false },
            { text: '', isCorrect: false },
          ],
        },
      ],
    }));
  }

  protected removeQuestion(questionIndex: number): void {
    this.model.update((model) => ({
      ...model,
      questions: model.questions.filter((_, index) => index !== questionIndex),
    }));
  }

  protected addAnswer(questionIndex: number): void {
    this.model.update((model) => ({
      ...model,
      questions: model.questions.map((question, index) =>
        index === questionIndex
          ? { ...question, answers: [...question.answers, { text: '', isCorrect: false }] }
          : question,
      ),
    }));
  }

  protected removeAnswer(questionIndex: number, answerIndex: number): void {
    this.model.update((model) => ({
      ...model,
      questions: model.questions.map((question, index) =>
        index === questionIndex
          ? { ...question, answers: question.answers.filter((_, i) => i !== answerIndex) }
          : question,
      ),
    }));
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    const xp = this.form.xp().value();
    const latitude = this.form.latitude().value();
    const longitude = this.form.longitude().value();
    if (xp === null || latitude === null || longitude === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizAuthoring.update(this.id(), {
        name: this.form.name().value(),
        instructions: this.form.instructions().value(),
        xp,
        location: { latitude, longitude },
        questions: this.form.questions().value(),
      });
      await this.router.navigate(['/games/quizzes']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not change the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }
}

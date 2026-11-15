import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { CreateQuiz } from './quiz-authoring/create-quiz/create-quiz';
import { EditQuiz } from './quiz-authoring/edit-quiz/edit-quiz';
import { QuizList } from './quiz-authoring/quiz-list/quiz-list';
import { CreateSocialChallenge } from './social-challenge-authoring/create-social-challenge/create-social-challenge';
import { SocialChallengeList } from './social-challenge-authoring/social-challenge-list/social-challenge-list';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'challenges', component: SocialChallengeList },
  { path: 'challenges/create', component: CreateSocialChallenge },
  { path: 'quizzes', component: QuizList },
  { path: 'quizzes/create', component: CreateQuiz },
  { path: 'quizzes/:id/edit', component: EditQuiz },
];

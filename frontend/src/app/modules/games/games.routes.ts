import { Routes } from '@angular/router';
import { ChallengeOverviewList } from './challenge-overview/challenge-overview-list/challenge-overview-list';
import { GamesHome } from './games-home/games-home';
import { ActiveLocationChallenge } from './location-challenge-activation/active-location-challenge/active-location-challenge';
import { CreateLocationChallenge } from './location-challenge-authoring/create-location-challenge/create-location-challenge';
import { LocationChallengeDetails } from './location-challenge-authoring/location-challenge-details/location-challenge-details';
import { LocationChallengeList } from './location-challenge-authoring/location-challenge-list/location-challenge-list';
import { NearbyLocationChallenges } from './location-challenge-browsing/nearby-location-challenges/nearby-location-challenges';
import { CreateQuiz } from './quiz-authoring/create-quiz/create-quiz';
import { EditQuiz } from './quiz-authoring/edit-quiz/edit-quiz';
import { QuizList } from './quiz-authoring/quiz-list/quiz-list';
import { CreateSocialChallenge } from './social-challenge-authoring/create-social-challenge/create-social-challenge';
import { SocialChallengeList } from './social-challenge-authoring/social-challenge-list/social-challenge-list';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'overview', component: ChallengeOverviewList },
  { path: 'challenges', component: SocialChallengeList },
  { path: 'challenges/create', component: CreateSocialChallenge },
  { path: 'quizzes', component: QuizList },
  { path: 'quizzes/create', component: CreateQuiz },
  { path: 'quizzes/:id/edit', component: EditQuiz },
  { path: 'location-challenges', component: LocationChallengeList },
  { path: 'location-challenges/create', component: CreateLocationChallenge },
  { path: 'location-challenges/:id', component: LocationChallengeDetails },
  { path: 'nearby', component: NearbyLocationChallenges },
  { path: 'active/:id', component: ActiveLocationChallenge },
];

import { Routes } from '@angular/router';
import { CreateTour } from './tour-authoring/create-tour/create-tour';
import { MyTours } from './tour-authoring/my-tours/my-tours';
import { Simulator } from './position-simulator/simulator/simulator';
import { TourKeyPoints } from './tour-authoring/tour-key-points/tour-key-points';
import { TourList } from './tour-browsing/tour-list/tour-list';

export const explorationRoutes: Routes = [
  { path: '', component: TourList },
  { path: 'mine', component: MyTours },
  { path: 'mine/:id', component: TourKeyPoints },
  { path: 'create', component: CreateTour },
  { path: 'simulator', component: Simulator },
];

import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/landing-page/landing-page.component').then((m) => m.LandingPageComponent),
    pathMatch: 'full'
  },
  {
    path: 'artworks',
    loadComponent: () =>
      import('./pages/artworks-page/artworks-page.component').then((m) => m.ArtworksPageComponent)
  },
  {
    path: 'exhibits',
    loadComponent: () =>
      import('./pages/exhibits-page/exhibits-page.component').then((m) => m.ExhibitsPageComponent)
  },
  { path: '**', redirectTo: '' }
];

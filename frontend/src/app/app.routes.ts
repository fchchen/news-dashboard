import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.page').then(m => m.DashboardPage)
  },
  {
    path: 'hacker-news',
    loadComponent: () =>
      import('./features/hacker-news/hacker-news.page').then(m => m.HackerNewsPage)
  },
  {
    path: 'github-releases',
    loadComponent: () =>
      import('./features/github-releases/github-releases.page').then(m => m.GitHubReleasesPage)
  },
  {
    path: 'rss-feeds',
    loadComponent: () =>
      import('./features/rss-feeds/rss-feeds.page').then(m => m.RssFeedsPage)
  },
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];

import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { of, delay } from 'rxjs';
import { environment } from '../environments/environment';
import {
  mockDashboard,
  mockHnResponse,
  mockReleasesResponse,
  mockRssResponse,
  mockRssSources
} from './mock-data';

export const mockInterceptor: HttpInterceptorFn = (req, next) => {
  if (!environment.useMocks) {
    return next(req);
  }

  const url = req.url;

  if (url.includes('/api/dashboard')) {
    return of(new HttpResponse({ status: 200, body: mockDashboard })).pipe(delay(300));
  }

  if (url.includes('/api/hackernews')) {
    return of(new HttpResponse({ status: 200, body: mockHnResponse })).pipe(delay(200));
  }

  if (url.includes('/api/github/releases')) {
    return of(new HttpResponse({ status: 200, body: mockReleasesResponse })).pipe(delay(200));
  }

  if (url.includes('/api/rss/sources')) {
    return of(new HttpResponse({ status: 200, body: mockRssSources })).pipe(delay(100));
  }

  if (url.includes('/api/rss')) {
    return of(new HttpResponse({ status: 200, body: mockRssResponse })).pipe(delay(200));
  }

  if (url.includes('/api/news')) {
    return of(new HttpResponse({ status: 200, body: mockHnResponse })).pipe(delay(200));
  }

  return next(req);
};

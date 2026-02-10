import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, catchError, of, map, shareReplay } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  DashboardSummaryResponse,
  NewsItemDto,
  PagedResponse,
  TrendsResponse,
  RssFeedSourceDto
} from '../models/news.models';

@Injectable({ providedIn: 'root' })
export class NewsService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;
  private staticData = environment.staticData;

  isLoading = signal(false);
  error = signal<string | null>(null);

  // Caches for static data mode (fetch once, slice in memory)
  private cache = new Map<string, Observable<any>>();

  private getCached<T>(url: string): Observable<T> {
    if (!this.cache.has(url)) {
      this.cache.set(url, this.http.get<T>(url).pipe(shareReplay(1)));
    }
    return this.cache.get(url)!;
  }

  private staticPage(items: NewsItemDto[], page: number, pageSize: number): PagedResponse<NewsItemDto> {
    const sorted = [...items].sort((a, b) =>
      new Date(b.publishedAt).getTime() - new Date(a.publishedAt).getTime()
    );
    const start = (page - 1) * pageSize;
    return {
      items: sorted.slice(start, start + pageSize),
      totalCount: sorted.length,
      page,
      pageSize
    };
  }

  getDashboard(): Observable<DashboardSummaryResponse> {
    if (this.staticData) {
      return this.getCached<DashboardSummaryResponse>(`${this.apiUrl}/dashboard.json`);
    }
    return this.http.get<DashboardSummaryResponse>(`${this.apiUrl}/dashboard`);
  }

  getUnifiedFeed(page = 1, pageSize = 20, source?: string, company?: string): Observable<PagedResponse<NewsItemDto>> {
    if (this.staticData) {
      return this.getCached<PagedResponse<NewsItemDto>>(`${this.apiUrl}/hackernews.json`).pipe(
        map(data => {
          let items = data.items;
          if (source) items = items.filter(i => i.source === source);
          if (company) items = items.filter(i => i.company === company);
          return this.staticPage(items, page, pageSize);
        })
      );
    }
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (source) params = params.set('source', source);
    if (company) params = params.set('company', company);

    return this.http.get<PagedResponse<NewsItemDto>>(`${this.apiUrl}/news`, { params });
  }

  getTrends(): Observable<TrendsResponse> {
    if (this.staticData) {
      return this.getCached<TrendsResponse>(`${this.apiUrl}/trends.json`);
    }
    return this.http.get<TrendsResponse>(`${this.apiUrl}/news/trends`);
  }

  getHackerNews(page = 1, pageSize = 20): Observable<PagedResponse<NewsItemDto>> {
    if (this.staticData) {
      return this.getCached<PagedResponse<NewsItemDto>>(`${this.apiUrl}/hackernews.json`).pipe(
        map(data => this.staticPage(data.items, page, pageSize))
      );
    }
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<PagedResponse<NewsItemDto>>(`${this.apiUrl}/hackernews`, { params });
  }

  getGitHubReleases(page = 1, pageSize = 20): Observable<PagedResponse<NewsItemDto>> {
    if (this.staticData) {
      return this.getCached<PagedResponse<NewsItemDto>>(`${this.apiUrl}/github-releases.json`).pipe(
        map(data => this.staticPage(data.items, page, pageSize))
      );
    }
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<PagedResponse<NewsItemDto>>(`${this.apiUrl}/github/releases`, { params });
  }

  getRepoReleases(owner: string, repo: string, page = 1, pageSize = 20): Observable<PagedResponse<NewsItemDto>> {
    if (this.staticData) {
      // In static mode, filter from the full releases cache
      return this.getCached<PagedResponse<NewsItemDto>>(`${this.apiUrl}/github-releases.json`).pipe(
        map(data => {
          const filtered = data.items.filter(i =>
            i.url?.includes(`${owner}/${repo}`)
          );
          return this.staticPage(filtered, page, pageSize);
        })
      );
    }
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<PagedResponse<NewsItemDto>>(`${this.apiUrl}/github/releases/${owner}/${repo}`, { params });
  }

  getRssItems(page = 1, pageSize = 20, source?: string): Observable<PagedResponse<NewsItemDto>> {
    if (this.staticData) {
      return this.getCached<PagedResponse<NewsItemDto>>(`${this.apiUrl}/rss.json`).pipe(
        map(data => {
          let items = data.items;
          if (source) items = items.filter(i => i.metadata?.['feedName'] === source);
          return this.staticPage(items, page, pageSize);
        })
      );
    }
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (source) params = params.set('source', source);

    return this.http.get<PagedResponse<NewsItemDto>>(`${this.apiUrl}/rss`, { params });
  }

  getRssSources(): Observable<RssFeedSourceDto[]> {
    if (this.staticData) {
      return this.getCached<RssFeedSourceDto[]>(`${this.apiUrl}/rss-sources.json`);
    }
    return this.http.get<RssFeedSourceDto[]>(`${this.apiUrl}/rss/sources`);
  }

  refreshAll(): Observable<unknown> {
    if (this.staticData) {
      return of(null); // no-op in static mode
    }
    return this.http.post(`${this.apiUrl}/refresh`, {});
  }
}

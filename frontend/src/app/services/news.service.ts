import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
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

  isLoading = signal(false);
  error = signal<string | null>(null);

  getDashboard(): Observable<DashboardSummaryResponse> {
    return this.http.get<DashboardSummaryResponse>(`${this.apiUrl}/dashboard`);
  }

  getUnifiedFeed(page = 1, pageSize = 20, source?: string, company?: string): Observable<PagedResponse<NewsItemDto>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (source) params = params.set('source', source);
    if (company) params = params.set('company', company);

    return this.http.get<PagedResponse<NewsItemDto>>(`${this.apiUrl}/news`, { params });
  }

  getTrends(): Observable<TrendsResponse> {
    return this.http.get<TrendsResponse>(`${this.apiUrl}/news/trends`);
  }

  getHackerNews(page = 1, pageSize = 20): Observable<PagedResponse<NewsItemDto>> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<PagedResponse<NewsItemDto>>(`${this.apiUrl}/hackernews`, { params });
  }

  getGitHubReleases(page = 1, pageSize = 20): Observable<PagedResponse<NewsItemDto>> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<PagedResponse<NewsItemDto>>(`${this.apiUrl}/github/releases`, { params });
  }

  getRepoReleases(owner: string, repo: string, page = 1, pageSize = 20): Observable<PagedResponse<NewsItemDto>> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<PagedResponse<NewsItemDto>>(`${this.apiUrl}/github/releases/${owner}/${repo}`, { params });
  }

  getRssItems(page = 1, pageSize = 20, source?: string): Observable<PagedResponse<NewsItemDto>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (source) params = params.set('source', source);

    return this.http.get<PagedResponse<NewsItemDto>>(`${this.apiUrl}/rss`, { params });
  }

  getRssSources(): Observable<RssFeedSourceDto[]> {
    return this.http.get<RssFeedSourceDto[]>(`${this.apiUrl}/rss/sources`);
  }

  refreshAll(): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/refresh`, {});
  }
}

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatChipsModule, MatChipListboxChange } from '@angular/material/chips';
import { tap, catchError } from 'rxjs/operators';
import { of, forkJoin } from 'rxjs';
import { NewsService } from '../../services/news.service';
import { NewsItemDto, RssFeedSourceDto } from '../../models/news.models';
import { NewsCardComponent } from '../../shared/components/news-card/news-card.component';

@Component({
  selector: 'app-rss-feeds',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatPaginatorModule, MatChipsModule,
    NewsCardComponent
  ],
  templateUrl: './rss-feeds.page.html',
  styleUrl: './rss-feeds.page.scss'
})
export class RssFeedsPage implements OnInit {
  private newsService = inject(NewsService);

  items = signal<NewsItemDto[]>([]);
  sources = signal<RssFeedSourceDto[]>([]);
  totalCount = signal(0);
  isLoading = signal(true);
  page = signal(1);
  pageSize = signal(20);
  selectedSource = signal<string | undefined>(undefined);

  ngOnInit(): void {
    forkJoin([
      this.newsService.getRssSources(),
      this.newsService.getRssItems(this.page(), this.pageSize())
    ]).pipe(
      tap(([sources, response]) => {
        this.sources.set(sources);
        this.items.set(response.items);
        this.totalCount.set(response.totalCount);
        this.isLoading.set(false);
      }),
      catchError(() => {
        this.isLoading.set(false);
        return of(null);
      })
    ).subscribe();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.newsService.getRssItems(this.page(), this.pageSize(), this.selectedSource()).pipe(
      tap(response => {
        this.items.set(response.items);
        this.totalCount.set(response.totalCount);
        this.isLoading.set(false);
      }),
      catchError(() => {
        this.isLoading.set(false);
        return of(null);
      })
    ).subscribe();
  }

  onChipChange(event: MatChipListboxChange): void {
    const value = event.value === 'all' ? undefined : event.value;
    this.selectedSource.set(value);
    this.page.set(1);
    this.loadData();
  }

  onPageChange(event: PageEvent): void {
    this.page.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.loadData();
  }
}

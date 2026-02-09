import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatChipsModule } from '@angular/material/chips';
import { tap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { NewsService } from '../../services/news.service';
import { NewsItemDto } from '../../models/news.models';
import { CompanyBadgeComponent } from '../../shared/components/company-badge/company-badge.component';

@Component({
  selector: 'app-github-releases',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatPaginatorModule, MatChipsModule,
    CompanyBadgeComponent
  ],
  templateUrl: './github-releases.page.html',
  styleUrl: './github-releases.page.scss'
})
export class GitHubReleasesPage implements OnInit {
  private newsService = inject(NewsService);

  items = signal<NewsItemDto[]>([]);
  totalCount = signal(0);
  isLoading = signal(true);
  page = signal(1);
  pageSize = signal(20);

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.newsService.getGitHubReleases(this.page(), this.pageSize()).pipe(
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

  onPageChange(event: PageEvent): void {
    this.page.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.loadData();
  }
}

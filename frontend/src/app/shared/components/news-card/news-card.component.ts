import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { CompanyBadgeComponent } from '../company-badge/company-badge.component';
import { NewsItemDto } from '../../../models/news.models';

@Component({
  selector: 'app-news-card',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatChipsModule, MatIconModule, CompanyBadgeComponent],
  template: `
    <mat-card class="news-card">
      <mat-card-header>
        <mat-icon mat-card-avatar>{{ sourceIcon() }}</mat-icon>
        <mat-card-title>
          <a [href]="item().url" target="_blank" rel="noopener">{{ item().title }}</a>
        </mat-card-title>
        <mat-card-subtitle>
          <app-company-badge [company]="item().company" />
          <span class="source-label">{{ sourceLabel() }}</span>
          <span class="time-ago">{{ timeAgo() }}</span>
        </mat-card-subtitle>
      </mat-card-header>
      @if (item().description) {
        <mat-card-content>
          <p class="description">{{ item().description }}</p>
        </mat-card-content>
      }
      <mat-card-content>
        <div class="meta-row">
          @if (item().score > 0) {
            <span class="score"><mat-icon>arrow_upward</mat-icon> {{ item().score }}</span>
          }
          @if (item().metadata['commentCount']) {
            <span class="comments"><mat-icon>comment</mat-icon> {{ item().metadata['commentCount'] }}</span>
          }
          @if (item().metadata['version']) {
            <span class="version"><mat-icon>label</mat-icon> v{{ item().metadata['version'] }}</span>
          }
          @if (item().author) {
            <span class="author"><mat-icon>person</mat-icon> {{ item().author }}</span>
          }
        </div>
        @if (item().tags.length > 0) {
          <mat-chip-set class="tags">
            @for (tag of item().tags.slice(0, 5); track tag) {
              <mat-chip>{{ tag }}</mat-chip>
            }
          </mat-chip-set>
        }
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .news-card {
      margin-bottom: 12px;
    }
    mat-card-title a {
      color: inherit;
      text-decoration: none;
      &:hover { text-decoration: underline; }
    }
    mat-card-subtitle {
      display: flex;
      align-items: center;
      gap: 8px;
      flex-wrap: wrap;
    }
    .description {
      color: rgba(0,0,0,0.6);
      font-size: 14px;
      overflow: hidden;
      text-overflow: ellipsis;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
    }
    .meta-row {
      display: flex;
      gap: 16px;
      align-items: center;
      margin: 8px 0;
      font-size: 13px;
      color: rgba(0,0,0,0.6);
    }
    .meta-row span {
      display: flex;
      align-items: center;
      gap: 4px;
    }
    .meta-row mat-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
    }
    .tags { margin-top: 8px; }
    .source-label {
      font-size: 12px;
      color: rgba(0,0,0,0.5);
    }
    .time-ago {
      font-size: 12px;
      color: rgba(0,0,0,0.4);
    }
  `]
})
export class NewsCardComponent {
  item = input.required<NewsItemDto>();

  sourceIcon = computed(() => {
    switch (this.item().source) {
      case 'HackerNews': return 'whatshot';
      case 'GitHubRelease': return 'code';
      case 'RssFeed': return 'rss_feed';
      default: return 'article';
    }
  });

  sourceLabel = computed(() => {
    switch (this.item().source) {
      case 'HackerNews': return 'Hacker News';
      case 'GitHubRelease': return 'GitHub Release';
      case 'RssFeed': return this.item().metadata['feedName'] ?? 'RSS';
      default: return this.item().source;
    }
  });

  timeAgo = computed(() => {
    const date = new Date(this.item().publishedAt);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    const diffHr = Math.floor(diffMin / 60);
    const diffDays = Math.floor(diffHr / 24);

    if (diffMin < 1) return 'just now';
    if (diffMin < 60) return `${diffMin}m ago`;
    if (diffHr < 24) return `${diffHr}h ago`;
    if (diffDays < 30) return `${diffDays}d ago`;
    return date.toLocaleDateString();
  });
}

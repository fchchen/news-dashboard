import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-source-chip',
  standalone: true,
  imports: [CommonModule, MatChipsModule, MatIconModule],
  template: `
    <mat-chip-option [selectable]="false" class="source-chip">
      <mat-icon matChipAvatar>{{ iconName() }}</mat-icon>
      {{ label() }}
    </mat-chip-option>
  `,
  styles: [`
    .source-chip { font-size: 12px; }
  `]
})
export class SourceChipComponent {
  source = input<string>('');

  label(): string {
    switch (this.source()) {
      case 'HackerNews': return 'Hacker News';
      case 'GitHubRelease': return 'GitHub';
      case 'RssFeed': return 'RSS';
      default: return this.source();
    }
  }

  iconName(): string {
    switch (this.source()) {
      case 'HackerNews': return 'whatshot';
      case 'GitHubRelease': return 'code';
      case 'RssFeed': return 'rss_feed';
      default: return 'article';
    }
  }
}

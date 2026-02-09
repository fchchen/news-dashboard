import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { tap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { NewsService } from '../../services/news.service';
import { DashboardSummaryResponse } from '../../models/news.models';
import { CompanyBadgeComponent } from '../../shared/components/company-badge/company-badge.component';
import { NewsCardComponent } from '../../shared/components/news-card/news-card.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatCardModule, MatButtonModule, MatIconModule, MatChipsModule, MatProgressSpinnerModule,
    CompanyBadgeComponent, NewsCardComponent
  ],
  templateUrl: './dashboard.page.html',
  styleUrl: './dashboard.page.scss'
})
export class DashboardPage implements OnInit {
  private newsService = inject(NewsService);

  data = signal<DashboardSummaryResponse | null>(null);
  isLoading = signal(true);

  ngOnInit(): void {
    this.newsService.getDashboard().pipe(
      tap(data => {
        this.data.set(data);
        this.isLoading.set(false);
      }),
      catchError(() => {
        this.isLoading.set(false);
        return of(null);
      })
    ).subscribe();
  }
}

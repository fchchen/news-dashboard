import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'app-company-badge',
  standalone: true,
  imports: [CommonModule, MatChipsModule],
  template: `
    <span class="badge" [class]="'badge-' + company().toLowerCase()">
      {{ company() }}
    </span>
  `,
  styles: [`
    .badge {
      display: inline-block;
      padding: 2px 10px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 500;
      color: white;
    }
    .badge-anthropic { background: #d97706; }
    .badge-openai { background: #10a37f; }
    .badge-both { background: #7c3aed; }
    .badge-other { background: #6b7280; }
    .badge-various { background: #6b7280; }
  `]
})
export class CompanyBadgeComponent {
  company = input<string>('Other');
}

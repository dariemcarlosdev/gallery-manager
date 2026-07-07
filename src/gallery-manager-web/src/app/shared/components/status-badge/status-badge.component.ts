import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="badge" [attr.data-status]="status().toLowerCase()">{{ status() }}</span>`,
  styles: [`
    .badge {
      display: inline-block;
      padding: var(--space-1) var(--space-3);
      border-radius: var(--radius-pill);
      font-size: 0.72rem;
      font-weight: 600;
      letter-spacing: 0.06em;
      text-transform: uppercase;
    }

    .badge[data-status="available"] {
      background: var(--color-available-tint);
      color: var(--color-available);
    }

    .badge[data-status="onloan"] {
      background: var(--color-onloan-tint);
      color: var(--color-onloan);
    }

    .badge[data-status="sold"] {
      background: var(--color-sold-tint);
      color: var(--color-sold);
    }
  `]
})
export class StatusBadgeComponent {
  status = input.required<string>();
}

import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-gallery-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="gallery-card" [class.is-active]="active()">
      <span class="gallery-card__corner gallery-card__corner--tl" aria-hidden="true"></span>
      <span class="gallery-card__corner gallery-card__corner--br" aria-hidden="true"></span>
      <ng-content />
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }

    .gallery-card {
      position: relative;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
      padding: var(--space-5);
      border: 1px solid var(--color-line);
      border-radius: var(--radius-md);
      background: var(--color-surface);
      transition:
        box-shadow 0.3s var(--ease-premium),
        transform 0.3s var(--ease-premium),
        border-color 0.3s ease-out;

      &:hover {
        box-shadow: var(--shadow-card-hover);
        transform: translateY(-2px);

        .gallery-card__corner {
          opacity: 1;
        }
      }

      &.is-active {
        border-color: var(--color-accent);
      }

      @media (max-width: 640px) {
        flex-direction: column;
        align-items: stretch;
      }
    }

    .gallery-card__corner {
      position: absolute;
      width: 16px;
      height: 16px;
      opacity: 0;
      transition: opacity 0.3s var(--ease-premium);
      pointer-events: none;
    }

    .gallery-card__corner--tl {
      top: 6px;
      left: 6px;
      border-top: 2px solid var(--color-accent);
      border-left: 2px solid var(--color-accent);
    }

    .gallery-card__corner--br {
      bottom: 6px;
      right: 6px;
      border-bottom: 2px solid var(--color-accent);
      border-right: 2px solid var(--color-accent);
    }
  `]
})
export class GalleryCardComponent {
  active = input(false);
}

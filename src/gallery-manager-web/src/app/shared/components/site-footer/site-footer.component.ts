import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-site-footer',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <footer class="site-footer">
      <div class="site-footer__container">
        <a routerLink="/" class="site-footer__brand">
          <span class="site-footer__mark" aria-hidden="true">GM</span>
          <span class="site-footer__name">Gallery Manager</span>
        </a>

        <nav class="site-footer__links" aria-label="Footer">
          <a routerLink="/artworks">Artworks</a>
          <a routerLink="/exhibits">Exhibits</a>
          <a href="https://github.com/dariemcarlosdev/gallery-manager" target="_blank" rel="noopener noreferrer" class="site-footer__github">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0 0 24 12c0-6.63-5.37-12-12-12z"/></svg>
            <span>GitHub</span>
          </a>
        </nav>
      </div>

      <div class="site-footer__base">
        <p class="site-footer__copy">&copy; 2026 Gallery Manager · A portfolio project by Dariem Macias</p>
      </div>
    </footer>
  `,
  styles: [`
    .site-footer {
      border-top: 1px solid var(--color-line);
      background: var(--color-surface);
      padding: var(--space-7) var(--space-6) var(--space-5);

      @media (max-width: 768px) { padding: var(--space-6) var(--space-4) var(--space-4); }
    }

    .site-footer__container {
      max-width: 1080px;
      margin: 0 auto;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-5);
      padding-bottom: var(--space-5);
      border-bottom: 1px solid var(--color-line);

      @media (max-width: 560px) {
        flex-direction: column;
        align-items: flex-start;
        gap: var(--space-4);
      }
    }

    .site-footer__brand {
      display: inline-flex;
      align-items: center;
      gap: var(--space-3);
      text-decoration: none;
    }

    .site-footer__mark {
      display: grid;
      place-items: center;
      width: 32px;
      height: 32px;
      border-radius: var(--radius-sm);
      background: var(--color-accent);
      color: var(--color-bg);
      font-family: var(--font-display);
      font-weight: 600;
      font-size: 0.75rem;
      letter-spacing: 0.04em;
    }

    .site-footer__name {
      font-family: var(--font-display);
      font-size: 1.1rem;
      font-weight: 600;
      color: var(--color-ink);
    }

    .site-footer__links {
      display: flex;
      align-items: center;
      gap: var(--space-5);
      flex-wrap: wrap;

      a {
        color: var(--color-ink-muted);
        font-size: 0.9rem;
        font-weight: 500;
        text-decoration: none;
        transition: color 0.2s ease;

        &:hover { color: var(--color-accent); }
      }
    }

    .site-footer__github {
      display: inline-flex;
      align-items: center;
      gap: var(--space-2);

      svg { opacity: 0.7; transition: opacity 0.2s ease; }
      &:hover svg { opacity: 1; }
    }

    .site-footer__base {
      max-width: 1080px;
      margin: var(--space-5) auto 0;
    }

    .site-footer__copy {
      font-size: 0.75rem;
      color: rgba(140, 133, 124, 0.7);
      margin: 0;
      letter-spacing: 0.02em;
    }
  `]
})
/** Global site footer rendered on every route via the app shell. */
export class SiteFooterComponent {}

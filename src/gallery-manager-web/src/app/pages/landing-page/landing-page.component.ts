import { ChangeDetectionStrategy, Component, OnInit, OnDestroy, PLATFORM_ID, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { catchError, of } from 'rxjs';
import { ScrollRevealDirective } from '../../shared/directives/scroll-reveal.directive';
import { ArtworkService } from '../../services/artwork.service';
import { ExhibitService } from '../../services/exhibit.service';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [RouterLink, ScrollRevealDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.scss'
})
/** Marketing landing page: hero with live collection stats and a scroll-driven parallax. */
export class LandingPageComponent implements OnInit, OnDestroy {
  private platformId = inject(PLATFORM_ID);
  private artworks = inject(ArtworkService);
  private exhibits = inject(ExhibitService);

  private hero: HTMLElement | null = null;
  private rafId: number | null = null;
  private scrollHandler?: () => void;

  /** Live totals shown as hero stat badges; null while loading or on error. */
  readonly artworkCount = signal<number | null>(null);
  readonly exhibitCount = signal<number | null>(null);

  /** Fetches hero stats and wires a rAF-throttled scroll listener feeding --scroll-y. */
  ngOnInit(): void {
    this.artworks.getCount().pipe(catchError(() => of(null))).subscribe(c => this.artworkCount.set(c));
    this.exhibits.getCount().pipe(catchError(() => of(null))).subscribe(c => this.exhibitCount.set(c));

    if (!isPlatformBrowser(this.platformId)) return;

    this.hero = document.querySelector('.hero') as HTMLElement | null;

    this.scrollHandler = () => {
      if (!this.hero || this.rafId !== null) return;

      this.rafId = window.requestAnimationFrame(() => {
        this.rafId = null;
        this.hero?.style.setProperty('--scroll-y', String(window.scrollY));
      });
    };

    this.scrollHandler();
    window.addEventListener('scroll', this.scrollHandler, { passive: true });
  }

  /** Removes the scroll listener and cancels any pending frame. */
  ngOnDestroy(): void {
    if (this.scrollHandler) {
      window.removeEventListener('scroll', this.scrollHandler);
    }

    if (this.rafId !== null) {
      cancelAnimationFrame(this.rafId);
      this.rafId = null;
    }
  }
}

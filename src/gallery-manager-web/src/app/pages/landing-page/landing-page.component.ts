import { ChangeDetectionStrategy, Component, OnInit, OnDestroy, PLATFORM_ID, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { ScrollRevealDirective } from '../../shared/directives/scroll-reveal.directive';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [RouterLink, ScrollRevealDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.scss'
})
/** Marketing landing page with a scroll-driven hero parallax. */
export class LandingPageComponent implements OnInit, OnDestroy {
  private platformId = inject(PLATFORM_ID);
  private hero: HTMLElement | null = null;
  private rafId: number | null = null;
  private scrollHandler?: () => void;

  /** Wires a rAF-throttled scroll listener that feeds --scroll-y to the hero. */
  ngOnInit(): void {
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

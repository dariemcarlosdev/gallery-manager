import {
  Directive,
  ElementRef,
  Input,
  OnInit,
  OnDestroy,
  PLATFORM_ID,
  inject
} from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Directive({
  selector: '[appScrollReveal]',
  standalone: true
})
export class ScrollRevealDirective implements OnInit, OnDestroy {
  @Input() revealDelay = 0;
  @Input() revealDirection: 'up' | 'left' | 'right' | 'none' = 'up';

  private observer?: IntersectionObserver;
  private el = inject(ElementRef);
  private platformId = inject(PLATFORM_ID);

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const nativeEl = this.el.nativeElement as HTMLElement;

    const computedTransition = getComputedStyle(nativeEl).transition;
    const revealTransition = `opacity 0.8s cubic-bezier(0.22, 1, 0.36, 1) ${this.revealDelay}ms, translate 0.8s cubic-bezier(0.22, 1, 0.36, 1) ${this.revealDelay}ms`;

    nativeEl.style.opacity = '0';
    nativeEl.style.transition =
      computedTransition && computedTransition !== 'none' ? `${computedTransition}, ${revealTransition}` : revealTransition;

    const translateMap: Record<'up' | 'left' | 'right' | 'none', string> = {
      up: '0 40px',
      left: '-40px 0',
      right: '40px 0',
      none: '0 0'
    };
    nativeEl.style.setProperty('translate', translateMap[this.revealDirection]);

    this.observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          nativeEl.style.opacity = '1';
          nativeEl.style.setProperty('translate', '0 0');
          this.observer?.unobserve(nativeEl);
        }
      },
      { threshold: 0.15, rootMargin: '0px 0px -50px 0px' }
    );

    this.observer.observe(nativeEl);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }
}

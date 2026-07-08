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
    nativeEl.style.opacity = '0';
    nativeEl.style.transition = `opacity 0.8s cubic-bezier(0.22, 1, 0.36, 1) ${this.revealDelay}ms, transform 0.8s cubic-bezier(0.22, 1, 0.36, 1) ${this.revealDelay}ms`;

    const translateMap = {
      up: 'translateY(40px)',
      left: 'translateX(-40px)',
      right: 'translateX(40px)',
      none: 'none'
    };
    nativeEl.style.transform = translateMap[this.revealDirection];

    this.observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          nativeEl.style.opacity = '1';
          nativeEl.style.transform = 'none';
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

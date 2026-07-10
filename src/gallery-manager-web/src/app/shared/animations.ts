import {
  animate,
  animateChild,
  query,
  stagger,
  style,
  transition,
  trigger
} from '@angular/animations';

/** Shared easing curve for all triggers. */
const EASE_PREMIUM = 'cubic-bezier(0.22, 1, 0.36, 1)';

/** Fade + slide on route change. */
export const routeFade = trigger('routeFade', [
  transition('* <=> *', [
    style({ opacity: 0, transform: 'translateY(8px)' }),
    animate(`280ms ${EASE_PREMIUM}`, style({ opacity: 1, transform: 'translateY(0)' }))
  ])
]);

/** Staggers child @listItem enter animations across a list. */
export const listStagger = trigger('listStagger', [
  transition('* <=> *', [
    query('@listItem', [stagger(60, animateChild())], { optional: true })
  ])
]);

/** Per-item enter animation (fade + rise + scale). */
export const listItem = trigger('listItem', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateY(12px) scale(0.98)' }),
    animate(`320ms ${EASE_PREMIUM}`, style({ opacity: 1, transform: 'translateY(0) scale(1)' }))
  ])
]);

/** Enter/leave reveal for panels (forms, revenue box). */
export const panelReveal = trigger('panelReveal', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateY(-6px) scale(0.99)' }),
    animate(`250ms ${EASE_PREMIUM}`, style({ opacity: 1, transform: 'translateY(0) scale(1)' }))
  ]),
  transition(':leave', [
    animate('180ms ease-in', style({ opacity: 0, transform: 'scale(0.98)' }))
  ])
]);

/** Simple fade-in on enter. */
export const fadeIn = trigger('fadeIn', [
  transition(':enter', [
    style({ opacity: 0 }),
    animate('200ms ease-out', style({ opacity: 1 }))
  ])
]);

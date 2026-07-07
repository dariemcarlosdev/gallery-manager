import {
  animate,
  animateChild,
  query,
  stagger,
  style,
  transition,
  trigger
} from '@angular/animations';

/** Route content: fade + rise on enter. Attach to the element wrapping <router-outlet>. */
export const routeFade = trigger('routeFade', [
  transition('* <=> *', [
    style({ opacity: 0, transform: 'translateY(6px)' }),
    animate('220ms cubic-bezier(0.22, 1, 0.36, 1)', style({ opacity: 1, transform: 'translateY(0)' }))
  ])
]);

/** List container: staggers each direct child's entrance. Pair with listItem on children. */
export const listStagger = trigger('listStagger', [
  transition('* <=> *', [
    query('@listItem', [stagger(45, animateChild())], { optional: true })
  ])
]);

export const listItem = trigger('listItem', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateY(10px)' }),
    animate('260ms cubic-bezier(0.22, 1, 0.36, 1)', style({ opacity: 1, transform: 'translateY(0)' }))
  ])
]);

/** Panel reveal: for banners/revenue panels appearing conditionally. */
export const panelReveal = trigger('panelReveal', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateY(-4px)' }),
    animate('200ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
  ]),
  transition(':leave', [
    animate('150ms ease-in', style({ opacity: 0 }))
  ])
]);

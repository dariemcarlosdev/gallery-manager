import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { routeFade } from './shared/animations';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  animations: [routeFade]
})
/** Root shell: nav chrome plus the routed outlet with fade transitions. */
export class AppComponent {
  private router = inject(Router);

  /** True when the landing route ('/') is active — used to swap nav chrome. */
  get isLanding(): boolean {
    return this.router.isActive('/', {
      paths: 'exact',
      queryParams: 'ignored',
      fragment: 'ignored',
      matrixParams: 'ignored'
    });
  }

  /** Returns the active route's path, keying the outlet animation per route. */
  outletState(outlet: RouterOutlet): string {
    return outlet.isActivated ? outlet.activatedRoute.routeConfig?.path ?? '' : '';
  }
}

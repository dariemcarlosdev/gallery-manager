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
export class AppComponent {
  private router = inject(Router);

  get isLanding(): boolean {
    return this.router.parseUrl(this.router.url).root.children['primary'] === undefined;
  }

  outletState(outlet: RouterOutlet): string {
    return outlet.isActivated ? outlet.activatedRoute.routeConfig?.path ?? '' : '';
  }
}

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
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
  outletState(outlet: RouterOutlet): string {
    return outlet.isActivated ? outlet.activatedRoute.routeConfig?.path ?? '' : '';
  }
}

import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ExhibitService } from '../../services/exhibit.service';
import { Exhibit, ExhibitRevenue } from '../../models/exhibit.model';
import { listItem, listStagger, panelReveal } from '../../shared/animations';
import { GalleryCardComponent } from '../../shared/components/gallery-card/gallery-card.component';

@Component({
  selector: 'app-exhibits-page',
  standalone: true,
  imports: [FormsModule, CurrencyPipe, GalleryCardComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './exhibits-page.component.html',
  styleUrl: './exhibits-page.component.scss',
  animations: [listStagger, listItem, panelReveal]
})
export class ExhibitsPageComponent {
  private readonly exhibitService = inject(ExhibitService);
  private readonly destroyRef = inject(DestroyRef);

  readonly exhibits = signal<Exhibit[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly assignArtworkIdByExhibit = signal<Record<number, number | null>>({});
  readonly assigning = signal<number | null>(null);

  readonly activeExhibitId = signal<number | null>(null);
  readonly revenue = signal<ExhibitRevenue | null>(null);
  readonly revenueLoading = signal(false);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.exhibitService
      .getExhibits()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (exhibits) => {
          this.exhibits.set(exhibits);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Could not load exhibits. Is the API running?');
          this.loading.set(false);
        }
      });
  }

  artworkIdFor(exhibitId: number): number | null {
    return this.assignArtworkIdByExhibit()[exhibitId] ?? null;
  }

  onArtworkIdChange(exhibitId: number, value: string): void {
    const parsed = value ? Number(value) : null;
    this.assignArtworkIdByExhibit.update((map) => ({ ...map, [exhibitId]: parsed }));
  }

  onAssign(exhibit: Exhibit): void {
    const artworkId = this.artworkIdFor(exhibit.id);
    if (!artworkId) {
      return;
    }

    this.assigning.set(exhibit.id);
    this.exhibitService
      .assignArtwork(exhibit.id, artworkId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.assigning.set(null);
          this.assignArtworkIdByExhibit.update((map) => ({ ...map, [exhibit.id]: null }));
          this.load();
        },
        error: () => {
          this.assigning.set(null);
          this.error.set(`Could not assign artwork #${artworkId} to "${exhibit.name}".`);
        }
      });
  }

  onViewRevenue(exhibit: Exhibit): void {
    this.activeExhibitId.set(exhibit.id);
    this.revenue.set(null);
    this.revenueLoading.set(true);
    this.exhibitService
      .getRevenue(exhibit.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (revenue) => {
          this.revenue.set(revenue);
          this.revenueLoading.set(false);
        },
        error: () => {
          this.revenueLoading.set(false);
          this.error.set(`Could not load revenue for "${exhibit.name}".`);
        }
      });
  }
}

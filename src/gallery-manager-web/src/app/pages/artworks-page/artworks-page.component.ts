import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ArtworkService } from '../../services/artwork.service';
import { Artwork, ArtworkStatus, CreateArtworkRequest } from '../../models/artwork.model';
import { listItem, listStagger, panelReveal } from '../../shared/animations';
import { GalleryCardComponent } from '../../shared/components/gallery-card/gallery-card.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

/** Blank create-form draft, reused after each successful create. */
const EMPTY_DRAFT: CreateArtworkRequest = { title: '', artist: '', medium: '', price: 0 };

@Component({
  selector: 'app-artworks-page',
  standalone: true,
  imports: [FormsModule, CurrencyPipe, GalleryCardComponent, StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './artworks-page.component.html',
  styleUrl: './artworks-page.component.scss',
  animations: [listStagger, listItem, panelReveal]
})
/** Artworks page: lists, filters, creates artworks, and drives status changes. */
export class ArtworksPageComponent {
  private readonly artworkService = inject(ArtworkService);
  private readonly destroyRef = inject(DestroyRef);

  readonly statuses: ArtworkStatus[] = ['Available', 'OnLoan', 'Sold'];

  readonly artworks = signal<Artwork[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly statusFilter = signal<ArtworkStatus | ''>('');

  readonly draft = signal<CreateArtworkRequest>({ ...EMPTY_DRAFT });
  readonly saving = signal(false);

  readonly pendingExhibitIdByArtwork = signal<Record<number, number | null>>({});
  readonly pendingStatusByArtwork = signal<Record<number, ArtworkStatus>>({});

  constructor() {
    this.load();
  }

  /** Loads artworks using the current status filter. */
  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.artworkService
      .getArtworks(this.statusFilter() || undefined)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (artworks) => {
          this.artworks.set(artworks);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Could not load artworks. Is the API running?');
          this.loading.set(false);
        }
      });
  }

  /** Applies a new status filter and reloads. */
  onFilterChange(status: ArtworkStatus | ''): void {
    this.statusFilter.set(status);
    this.load();
  }

  /** Immutably updates one field of the create-form draft. */
  setDraftField<K extends keyof CreateArtworkRequest>(field: K, value: CreateArtworkRequest[K]): void {
    this.draft.update((current) => ({ ...current, [field]: value }));
  }

  /** Validates the draft and creates the artwork, then resets and reloads. */
  onCreate(): void {
    const request = this.draft();
    if (!request.title || !request.artist || !request.medium || request.price < 0) {
      return;
    }

    this.saving.set(true);
    this.artworkService
      .createArtwork(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.draft.set({ ...EMPTY_DRAFT });
          this.load();
        },
        error: () => {
          this.saving.set(false);
          this.error.set('Could not create artwork — check the fields and try again.');
        }
      });
  }

  /** Pending exhibit id entered for an artwork's OnLoan transition. */
  exhibitIdFor(artworkId: number): number | null {
    return this.pendingExhibitIdByArtwork()[artworkId] ?? null;
  }

  /** Records the exhibit id typed for an artwork. */
  onExhibitIdChange(artworkId: number, value: string): void {
    const parsed = value ? Number(value) : null;
    this.pendingExhibitIdByArtwork.update((map) => ({ ...map, [artworkId]: parsed }));
  }

  /** Pending status selection for an artwork, defaulting to its current status. */
  pendingStatusFor(artwork: Artwork): ArtworkStatus {
    return this.pendingStatusByArtwork()[artwork.id] ?? artwork.status;
  }

  /** Records a status choice; commits immediately unless it's OnLoan. */
  onStatusSelect(artwork: Artwork, status: ArtworkStatus): void {
    this.pendingStatusByArtwork.update((map) => ({ ...map, [artwork.id]: status }));

    // Non-loan statuses need no extra data, so commit right away. OnLoan waits
    // for the exhibit ID and an explicit confirm — see onConfirmOnLoan.
    if (status !== 'OnLoan') {
      this.commitStatus(artwork, status, null);
    }
  }

  /** Confirms an OnLoan transition once an exhibit id is supplied. */
  onConfirmOnLoan(artwork: Artwork): void {
    const exhibitId = this.exhibitIdFor(artwork.id);
    if (!exhibitId) {
      return;
    }
    this.commitStatus(artwork, 'OnLoan', exhibitId);
  }

  /** Sends the status update and reloads on success. */
  private commitStatus(artwork: Artwork, status: ArtworkStatus, exhibitId: number | null): void {
    this.artworkService
      .updateStatus(artwork.id, { status, exhibitId })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.load(),
        error: () => this.error.set(`Could not update status for "${artwork.title}".`)
      });
  }
}

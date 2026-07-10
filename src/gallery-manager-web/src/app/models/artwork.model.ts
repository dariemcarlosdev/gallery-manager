/** Lifecycle state of an artwork. */
export type ArtworkStatus = 'Available' | 'OnLoan' | 'Sold';

/** Artwork as returned by the API. */
export interface Artwork {
  id: number;
  title: string;
  artist: string;
  medium: string;
  price: number;
  status: ArtworkStatus;
  /** Owning exhibit when on loan; null otherwise. */
  exhibitId: number | null;
}

/** Payload to create an artwork (POST /artworks). */
export interface CreateArtworkRequest {
  title: string;
  artist: string;
  medium: string;
  price: number;
}

/** Response after creating an artwork. */
export interface CreateArtworkResponse {
  id: number;
  title: string;
  artist: string;
  medium: string;
  price: number;
  status: ArtworkStatus;
}

/** Payload to change an artwork's status (PATCH /artworks/{id}/status). */
export interface UpdateArtworkStatusRequest {
  status: ArtworkStatus;
  /** Required when status is OnLoan; null otherwise. */
  exhibitId: number | null;
}

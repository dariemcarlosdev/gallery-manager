export type ArtworkStatus = 'Available' | 'OnLoan' | 'Sold';

export interface Artwork {
  id: number;
  title: string;
  artist: string;
  medium: string;
  price: number;
  status: ArtworkStatus;
  exhibitId: number | null;
}

export interface CreateArtworkRequest {
  title: string;
  artist: string;
  medium: string;
  price: number;
}

export interface CreateArtworkResponse {
  id: number;
  title: string;
  artist: string;
  medium: string;
  price: number;
  status: ArtworkStatus;
}

export interface UpdateArtworkStatusRequest {
  status: ArtworkStatus;
  exhibitId: number | null;
}

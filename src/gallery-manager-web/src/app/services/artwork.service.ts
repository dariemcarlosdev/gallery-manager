import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Artwork,
  ArtworkStatus,
  CreateArtworkRequest,
  CreateArtworkResponse,
  UpdateArtworkStatusRequest
} from '../models/artwork.model';
import { PagedResponse } from '../models/paged-response.model';

@Injectable({ providedIn: 'root' })
export class ArtworkService {
  private readonly baseUrl = `${environment.apiUrl}/artworks`;

  constructor(private readonly http: HttpClient) {}

  getArtworks(status?: ArtworkStatus): Observable<Artwork[]> {
    const params: Record<string, string> = {};
    if (status) params['status'] = status;
    return this.http
      .get<PagedResponse<Artwork>>(this.baseUrl, { params })
      .pipe(map(res => res.data));
  }

  createArtwork(request: CreateArtworkRequest): Observable<CreateArtworkResponse> {
    return this.http.post<CreateArtworkResponse>(this.baseUrl, request);
  }

  updateStatus(id: number, request: UpdateArtworkStatusRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/status`, request);
  }
}

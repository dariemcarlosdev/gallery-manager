import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Exhibit, ExhibitRevenue } from '../models/exhibit.model';
import { PagedResponse } from '../models/paged-response.model';

/** HTTP client for the exhibits API. */
@Injectable({ providedIn: 'root' })
export class ExhibitService {
  private readonly baseUrl = `${environment.apiUrl}/exhibits`;

  constructor(private readonly http: HttpClient) {}

  /** Lists exhibits; unwraps the paged envelope. */
  getExhibits(): Observable<Exhibit[]> {
    return this.http
      .get<PagedResponse<Exhibit>>(this.baseUrl)
      .pipe(map(res => res.data));
  }

  /** Returns the total exhibit count from the paged envelope. */
  getCount(): Observable<number> {
    return this.http
      .get<PagedResponse<Exhibit>>(this.baseUrl)
      .pipe(map(res => res.totalCount));
  }

  /** Assigns an artwork to an exhibit. */
  assignArtwork(exhibitId: number, artworkId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${exhibitId}/artworks/${artworkId}`, {});
  }

  /** Fetches the revenue breakdown for an exhibit. */
  getRevenue(exhibitId: number): Observable<ExhibitRevenue> {
    return this.http.get<ExhibitRevenue>(`${this.baseUrl}/${exhibitId}/revenue`);
  }
}

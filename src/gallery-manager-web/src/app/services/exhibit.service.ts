import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Exhibit, ExhibitRevenue } from '../models/exhibit.model';

@Injectable({ providedIn: 'root' })
export class ExhibitService {
  private readonly baseUrl = `${environment.apiUrl}/exhibits`;

  constructor(private readonly http: HttpClient) {}

  getExhibits(): Observable<Exhibit[]> {
    return this.http.get<Exhibit[]>(this.baseUrl);
  }

  assignArtwork(exhibitId: number, artworkId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${exhibitId}/artworks/${artworkId}`, {});
  }

  getRevenue(exhibitId: number): Observable<ExhibitRevenue> {
    return this.http.get<ExhibitRevenue>(`${this.baseUrl}/${exhibitId}/revenue`);
  }
}

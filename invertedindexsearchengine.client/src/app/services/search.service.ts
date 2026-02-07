import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private apiUrl = 'https://localhost:7269/api/search';

  constructor(private http: HttpClient) {}

  search(query: string) {
    return this.http.get(`${this.apiUrl}/search?q=${query}`);
  }
}

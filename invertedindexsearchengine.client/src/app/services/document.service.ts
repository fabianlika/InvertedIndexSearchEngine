import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private apiUrl = 'https://localhost:7269/api/search';

  constructor(private http: HttpClient) {}

  // Tell Angular that the response is plain text
  uploadDocument(title: string, content: string) {
    return this.http.post(`${this.apiUrl}/upload`, { title, content }, { responseType: 'text' });
  }

  seedDocuments() {
    return this.http.post(`${this.apiUrl}/seed`, {}, { responseType: 'text' });
  }

  uploadDocumentFile(title: string, file: File) {
    const formData = new FormData();
    formData.append('title', title);
    formData.append('file', file);

    return this.http.post(`${this.apiUrl}/upload-file`, formData, { responseType: 'text' });
  }

  
}

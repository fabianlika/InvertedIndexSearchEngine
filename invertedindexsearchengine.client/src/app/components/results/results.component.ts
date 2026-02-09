import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-results',
  templateUrl: './results.component.html',
  styleUrls: ['./results.component.css']
})
export class ResultsComponent {
  @Input() results: any[] = [];
  @Input() query: string = '';

  // The document currently being viewed in the modal
  selectedDoc: any = null;

  // Open the modal
  openModal(doc: any) {
    this.selectedDoc = doc;
    // Prevent background scrolling
    document.body.style.overflow = 'hidden'; 
  }

  // Close the modal
  closeModal() {
    this.selectedDoc = null;
    // Restore background scrolling
    document.body.style.overflow = 'auto'; 
  }

  // Helper to highlight terms in the text
  getHighlightedContent(text: string): string {
    if (!this.query || !text) return text;
    
    // Simple logic to wrap query terms in <mark> tags
    const pattern = new RegExp(`(${this.query})`, 'gi');
    return text.replace(pattern, '<mark>$1</mark>');
  }
}
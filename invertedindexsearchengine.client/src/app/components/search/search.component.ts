import { Component } from '@angular/core';
import { SearchService } from '../../services/search.service';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent {
  query: string = '';
  results: any[] = [];
  
  // State for the modal overlay
  selectedDoc: any = null;
   hasSearched = false; // ✅ NEW

  constructor(private searchService: SearchService) {}

 search() {
    if (!this.query.trim()) return;

    this.hasSearched = true; // ✅ Mark search triggered
    
    this.searchService.search(this.query).subscribe({
      next: (res: any) => {
        this.results = res;
        this.selectedDoc = null;
      },
      error: (err) => console.error(err)
    });
  }
  // --- MODAL LOGIC ---
 openModal(doc: any) {
  
  this.selectedDoc = { ...doc, loading: true };
  document.body.style.overflow = 'hidden';
 console.log(`Fetching full content for document ID: ${doc.id}`);
  this.searchService.getDocument(doc.id).subscribe({
    next: fullDoc => {
      this.selectedDoc = { ...doc, ...fullDoc, loading: false };
    },
    error: () => {
      this.selectedDoc.loading = false;
    }
  });
}


  closeModal() {
    this.selectedDoc = null;
    document.body.style.overflow = 'auto'; // Restore scrolling
  }

  // Highlights text for both Snippets and Full Content
  highlightQuery(text: string): string {
    if (!this.query || !text) return text;

    const terms = this.query.split(' ').filter(t => t.length > 0);
    let highlighted = text;

    terms.forEach(term => {
      // Create a regex that matches the term (case insensitive)
      const regex = new RegExp(`(${term})`, 'gi');
      // Wrap the match in a styled span
      highlighted = highlighted.replace(regex, '<span class="highlight">$1</span>');
    });

    return highlighted;
  }

  onQueryChange() {
  this.hasSearched = false;
  this.results = []; // optional: clears old results visually
}


  
}
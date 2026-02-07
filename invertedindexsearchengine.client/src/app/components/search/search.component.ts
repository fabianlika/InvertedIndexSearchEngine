import { Component } from '@angular/core';
import { SearchService } from '../../services/search.service';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent {
  query = '';
  results: any[] = [];

  constructor(private searchService: SearchService) {}

  search() {
    if (!this.query.trim()) return;
    this.searchService.search(this.query).subscribe({
      next: res => this.results = res as any[],
      error: err => console.error(err)
    });
  }

  highlightQuery(text: string): string {
  if (!this.query) return text;

  const terms = this.query.split(' ').filter(t => t.length > 0);
  let highlighted = text;

  terms.forEach(term => {
    const regex = new RegExp(`(${term})`, 'gi');
    highlighted = highlighted.replace(regex, '<span class="highlight">$1</span>');
  });

  return highlighted;
}


}

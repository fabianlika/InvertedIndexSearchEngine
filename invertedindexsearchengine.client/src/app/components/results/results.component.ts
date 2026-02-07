import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-results',
  templateUrl: './results.component.html',
  styleUrls: ['./results.component.css']
})
export class ResultsComponent {
  @Input() results: any[] = [];
  @Input() query: string = '';

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

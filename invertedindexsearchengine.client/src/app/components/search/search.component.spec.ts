<div class="card shadow-lg rounded-4 p-4 mx-auto" style="max-width: 800px;">
  <h2 class="mb-4 text-center">Search Documents</h2>

  <div class="input-group mb-3">
    <input type="text" [(ngModel)]="query" placeholder="Type your search..." class="form-control form-control-lg rounded-start shadow-sm" />
    <button class="btn btn-primary btn-lg rounded-end shadow-sm" type="button" (click)="search()">Search</button>
  </div>

  <app-results [results]="results"></app-results>
</div>

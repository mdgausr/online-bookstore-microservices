import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-catalog',
  template: `
    <h2>Catalog</h2>
    <div *ngIf="books?.length === 0">No books available.</div>
    <div class="books">
      <mat-card *ngFor="let b of books" style="margin:8px; width:280px; display:inline-block; vertical-align:top;">
        <mat-card-title>{{b.title}}</mat-card-title>
        <mat-card-subtitle>{{b.author}}</mat-card-subtitle>
        <mat-card-content>
          <p>{{b.description}}</p>
          <p><strong>{{b.price | currency}}</strong></p>
        </mat-card-content>
        <mat-card-actions>
          <button mat-button color="primary" [routerLink]="['/product', b.id]">View</button>
          <button mat-button color="accent" (click)="addToCart(b)">Add to Cart</button>
        </mat-card-actions>
      </mat-card>
    </div>
  `
})
export class CatalogComponent implements OnInit {
  books: any[] = [];
  constructor(private api: ApiService) {}
  ngOnInit() { this.api.getBooks().subscribe(b => this.books = b as any[]); }
  addToCart(book: any) { this.api.addToCart(book.id, 1).subscribe(); alert('Added to cart'); }
}

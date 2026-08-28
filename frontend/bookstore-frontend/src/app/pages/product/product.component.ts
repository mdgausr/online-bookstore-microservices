import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-product',
  template: `
    <div *ngIf="product">
      <h2>{{product.title}}</h2>
      <p><strong>Author:</strong> {{product.author}}</p>
      <p>{{product.description}}</p>
      <p><strong>{{product.price | currency}}</strong></p>
      <button mat-raised-button color="primary" (click)="addToCart()">Add to Cart</button>
    </div>
  `
})
export class ProductComponent implements OnInit {
  product: any;
  id!: number;
  constructor(private route: ActivatedRoute, private api: ApiService){}
  ngOnInit(){ this.id = Number(this.route.snapshot.paramMap.get('id'));
    this.api.getBook(this.id).subscribe(b => this.product = b);
  }
  addToCart(){ this.api.addToCart(this.product.id,1).subscribe(()=>alert('Added to cart')); }
}

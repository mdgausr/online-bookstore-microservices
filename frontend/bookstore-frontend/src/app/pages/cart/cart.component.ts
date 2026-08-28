import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-cart',
  template: `
    <h2>Your Cart</h2>
    <div *ngIf="items?.length === 0">Cart is empty</div>
    <mat-list>
      <mat-list-item *ngFor="let i of items">
        <div mat-line>Product ID: {{i.bookId}}</div>
        <div mat-line>Quantity: {{i.quantity}}</div>
      </mat-list-item>
    </mat-list>
    <button mat-raised-button color="primary" routerLink="/checkout" *ngIf="items.length>0">Checkout</button>
  `
})
export class CartComponent implements OnInit{
  items: any[] = [];
  userId = '00000000-0000-0000-0000-000000000001';
  constructor(private api: ApiService){}
  ngOnInit(){ this.load(); }
  load(){ this.api.getCart(this.userId).subscribe(r => this.items = r as any[]); }
}

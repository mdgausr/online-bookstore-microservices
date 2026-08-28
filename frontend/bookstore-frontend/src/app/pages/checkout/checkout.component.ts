import { Component } from '@angular/core';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-checkout',
  template: `
    <h2>Checkout</h2>
    <p>Simple checkout simulation — calls Orders API to create an order.</p>
    <button mat-raised-button color="primary" (click)="placeOrder()">Place Order</button>
  `
})
export class CheckoutComponent{
  userId = '00000000-0000-0000-0000-000000000001';
  constructor(private api: ApiService){}
  placeOrder(){ this.api.createOrder(this.userId, 100).subscribe(r => alert('Order created: ' + r.orderId)); }
}

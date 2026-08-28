import { Component } from '@angular/core';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-checkout',
  template: `
    <h2>Checkout</h2>
    <p *ngIf="!orderId">Simple checkout simulation — calls Orders API to create an order.</p>
    <p *ngIf="orderId">Order ID: {{orderId}}</p>
    <p *ngIf="status">Status: {{status}}</p>
    <button mat-raised-button color="primary" (click)="placeOrder()" *ngIf="!orderId">Place Order</button>
    <button mat-raised-button color="warn" (click)="reset()" *ngIf="orderId">New Order</button>
  `
})
export class CheckoutComponent{
  userId = '00000000-0000-0000-0000-000000000001';
  orderId: string | null = null;
  status: string | null = null;
  private pollHandle: any = null;
  constructor(private api: ApiService){}

  placeOrder(){
    this.api.createOrder(this.userId, 100).subscribe((r: any) => {
      this.orderId = r.orderId;
      this.status = 'Created';
      this.startPolling();
    });
  }

  startPolling(){
    if (!this.orderId) return;
    this.pollHandle = setInterval(() => {
      this.api.getOrderView(this.orderId as string).subscribe({ next: (res: any) => {
        this.status = res.status;
        if (this.status !== 'Created'){
          // stop polling when status changes to Paid or PaymentFailed
          if (this.pollHandle) { clearInterval(this.pollHandle); this.pollHandle = null; }
        }
      }, error: () => {
        // ignore not found during an initial window
      }});
    }, 2000);
  }

  reset(){
    if (this.pollHandle) { clearInterval(this.pollHandle); this.pollHandle = null; }
    this.orderId = null;
    this.status = null;
  }
}

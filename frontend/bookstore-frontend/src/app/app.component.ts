import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  template: `
    <mat-toolbar color="primary">
      <span style="cursor:pointer" routerLink="/">Online Bookstore</span>
      <span class="spacer"></span>
      <button mat-icon-button routerLink="/cart" aria-label="Cart">
        <mat-icon>shopping_cart</mat-icon>
      </button>
      <button mat-button routerLink="/account">Account</button>
      <button mat-button routerLink="/admin">Admin</button>
    </mat-toolbar>
    <div class="container">
      <router-outlet></router-outlet>
    </div>
  `,
  styles: ['.spacer { flex: 1 1 auto; }']
})
export class AppComponent { }

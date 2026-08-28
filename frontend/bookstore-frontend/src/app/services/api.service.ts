import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private base = environment.apiBaseUrl;
  private gateway = this.base;
  constructor(private http: HttpClient) {}

  getBooks(): Observable<any> { return this.http.get(this.gateway + 'catalog/api/books'); }
  getBook(id: number): Observable<any> { return this.http.get(this.gateway + 'catalog/api/books/' + id); }
  addToCart(bookId: number, qty: number){ const userId = '00000000-0000-0000-0000-000000000001'; return this.http.post(this.gateway + 'basket/api/basket/' + userId + '/items', { bookId, quantity: qty }); }
  getCart(userId: string){ return this.http.get(this.gateway + 'basket/api/basket/' + userId); }
  createOrder(userId: string, total: number){ return this.http.post(this.gateway + 'orders/api/orders', { userId, total }); }
}

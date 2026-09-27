import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

@Injectable({
   providedIn: 'root'
})
export class InvoiceService {

   constructor(private http: HttpClient, private authService: AuthService) { }


   createInvoice(orderId: number) {
      const token = this.authService.getToken();
      const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
      return this.http.post(environment.apiBaseUrl + '/CreateInvoiceByOrderId/' + orderId, { headers });
   }

   getInvoiceById(id: number) {
      const token = this.authService.getToken();
      const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
      return this.http.get(environment.apiBaseUrl + '/GetInvoiceById/' + id, { headers });
   }

   getInvoiceByOrderId(id: number) {
      const token = this.authService.getToken();
      const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
      return this.http.get(environment.apiBaseUrl + '/GetInvoiceByOrderId/' + id, { headers });
   }

   getAllInvoices() {
      const token = this.authService.getToken();
      const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
      return this.http.get(environment.apiBaseUrl + '/GetAllInvoice', { headers });
   }

   updateInvoiceById(id: number, invoice: any) {
      const token = this.authService.getToken();
      const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
      return this.http.put(environment.apiBaseUrl + '/UpdateInvoiceById/' + id, invoice, { headers });
   }

}

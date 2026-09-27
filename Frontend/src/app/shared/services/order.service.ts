import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  private http=inject(HttpClient);
  private authService=inject(AuthService);
  
  getOrderById(id:number)
  {
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.get(environment.apiBaseUrl+'/GetOrderById/'+id,{ headers });
  }

  getAllOrdersDto()
  {
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.get(environment.apiBaseUrl+'/GetAllOrderDto',{ headers });
  }

  createOrder(formData:any){
    return this.http.post(environment.apiBaseUrl+'/CreateOrder',formData);
  }


  updateOrderById(id:number,formData:any){
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.put(environment.apiBaseUrl+'/UpdateOrderById/'+id,formData,{ headers });
  }
  
  getOrderByCustomerId(customerId:string)
  {
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.get<any[]>(environment.apiBaseUrl+'/GetOrderByCustomerId/'+customerId,{ headers });
  }

  
}

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ServiceService {

  constructor(private http:HttpClient,private authService:AuthService) { }
    
    getAllServices()
    {
      const token = this.authService.getToken();
      const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
      return this.http.get(environment.apiBaseUrl+'/GetAllService',{ headers });
    }
  
    getServiceById(id:number)
    {
      const token = this.authService.getToken();
      const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
      return this.http.get(environment.apiBaseUrl+'/GetServiceById/'+id,{ headers });
    }
}

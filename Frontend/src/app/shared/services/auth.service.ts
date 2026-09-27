import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private http=inject(HttpClient)

  private tokenExpiredNotified = false;

  setTokenExpiredNotified(value: boolean) {
    this.tokenExpiredNotified = value;
  }

  wasTokenExpiredNotified(): boolean {
    return this.tokenExpiredNotified;
  }


  createUser(formData:any)
  { 
    return this.http.post(environment.apiBaseUrl+'/signup',formData)
  }

  updateUserById(id:string,formData:any)
  {
    return this.http.put(environment.apiBaseUrl+'/updateUserById/'+id,formData)
  }

  signin(formData:any)
  {
    return this.http.post(environment.apiBaseUrl+'/signin',formData)
  }

  
  IsLoggedIn()
  {
    return this.getToken()!=null?true:false;
  }

  saveTocken(token:string)
  {
    localStorage.setItem('TOKEN_KEY',token);
  }

  getToken()
  {
    return localStorage.getItem('TOKEN_KEY');
  }

  deleteToken()
  {
    localStorage.removeItem('TOKEN_KEY');
    this.tokenExpiredNotified=false;
  }

  getClaims()
  {
    return JSON.parse(window.atob(this.getToken()!.split('.')[1]));
  }

 
}

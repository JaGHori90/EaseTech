import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private http:HttpClient,private authService:AuthService) { }

  getUserProfile()
  {
    const token= this.authService.getToken();
    const headers= new HttpHeaders().set('Authorization','Bearer '+token);
    return this.http.get(environment.apiBaseUrl+'/userProfile', { headers });
  }

  getAllUserProfiles() {
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.get(environment.apiBaseUrl + '/GetAllUserProfile', { headers });
  }

  getAllCusotmer() {
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.get(environment.apiBaseUrl + '/GetAllCustomer', { headers });
  }

  getAllEmployeesAndAdmins() {
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.get(environment.apiBaseUrl + '/GetAllEmployeesAndAdmins', { headers });
  }

  getProfileById(id: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.get(environment.apiBaseUrl + '/GetUserProfileById/' + id, { headers });
  }

  changePassword(form:any)
  {
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.put(environment.apiBaseUrl+'/ChangePassword',form, { headers })
  }

  resetPassword(form:any)
  {
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.post(environment.apiBaseUrl+'/ResetPassword',form, { headers })
  }
  
  deleteProfile(form:any)
  {
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.post(environment.apiBaseUrl+'/DeleteUser',form,{headers})
  }
}

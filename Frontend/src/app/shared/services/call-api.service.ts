import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { UserService } from './user.service';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CallApiService {

  private http = inject(HttpClient);
  private userService = inject(UserService);
  private authService = inject(AuthService);

  GetCallByUserId(id: string) {
    {
      return this.http.get(environment.apiBaseUrl + '/GetCallByUserId/' + id);
    }
  }

  registerCall(from: any) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.post(environment.apiBaseUrl + '/RegisterCall', from, { headers });
  }

  update(id: number, form: any) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders().set('Authorization', 'Bearer ' + token);
    return this.http.put(environment.apiBaseUrl + '/UpdateCallsStatus/' + id, form, { headers });
  }

  GetToken() {
    return this.http.get(environment.apiBaseUrl + '/CreateToken');
  }


}

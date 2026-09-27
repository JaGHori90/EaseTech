import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RequestService {

  constructor(private http:HttpClient) { }

  createRequest(formData:any)
  {
    return this.http.post(environment.apiBaseUrl+'/CreateRequest',formData)
  }

  getAllRequests()
  {
    return this.http.get(environment.apiBaseUrl+'/GetAllRequest')
  }

  getRequestById(id:number)
  {
    return this.http.get(environment.apiBaseUrl+'/GetRequestById/'+id)
  }

  updateRequest(id:number,formData:any)
  {
    return this.http.put(environment.apiBaseUrl+'/UpdateRequestById/'+id,formData)
  }
}

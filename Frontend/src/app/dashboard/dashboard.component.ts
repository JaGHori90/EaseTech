import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { UserService } from '../shared/services/user.service';
import { CommonModule } from '@angular/common';
import { HideIfClaimsNotMetDirective } from '../shared/directives/hide-if-claims-not-met.directive';
import { claimReq } from '../shared/utils/claimReq-utils';
import { RouterLink } from '@angular/router';
import { RequestService } from '../shared/services/request.service';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { CallApiService } from '../shared/services/call-api.service';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';


@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule,FormsModule, ReactiveFormsModule, HideIfClaimsNotMetDirective],
  templateUrl: './dashboard.component.html',
  styles: ''
})
export class DashboardComponent implements OnInit {

  private callService = inject(CallApiService);
  private userService = inject(UserService);
  private formBuilder = inject(FormBuilder);
  private toaster = inject(ToastrService);

  claimReq = claimReq;
  private intervalId: any;
  lastName: string = '';
  fistName: string = '';
  userId:string=''
  isSubmitted: boolean = false;
  callId:number=0;
  form:FormGroup;
  userIsRegistered: boolean = false;

  ngOnInit() 
    {
      this.loadUserProfile();
    }
    
    constructor() {
       this.form = this.formBuilder.group({
            callId: ['', [Validators.required, Validators.minLength(10)]],
            isAvailable: [false, [Validators.required]],
          });
    }

    hasDisplayError(controlName: string): boolean {
      const control = this.form.get(controlName);
      return (
        Boolean(control?.invalid) &&
        (this.isSubmitted || Boolean(control?.touched) || Boolean(control?.dirty))
      );
    }

  loadUserProfile()
  {
    this.userService.getUserProfile().subscribe(
      {
        next: (res: any) => {
          this.lastName = res.lastName;
          this.fistName = res.firstName;
          this.userId = res.id;
          this.loadCall();
        },
        error: (err: any) => {
          console.log('error while retrieving user profile:\n', err);
        }
      }
    );
  }

  loadCall() {
    this.callService.GetCallByUserId(this.userId).subscribe({
      next: (data: any) => {
        this.callId= data.id;
        this.userIsRegistered=true;
        this.form.patchValue(
          {
            isAvailable: data.isAvailable,
            callId:data.callId,
          }
        );
      },
      error: (err: any) => {
        this.userIsRegistered=false;
        console.log('Error creating call' + err.error);
      }
    })
  }

  onSubmit() {
    this.isSubmitted = true;  

     if(this.form.valid && this.callId != 0)
     {
      const formData = this.form.value;
      formData.isAvailable = formData.isAvailable ==='true';

       this.callService.update(this.callId,formData).subscribe(
        {
          next:(data)=>
          {
            this.toaster.success('Die Update war erfolgreich')
          },
          error:(err:any)=>
          {
            console.log('Fehler beim Update'+err.error)
            this.toaster.error('Fehler beim Update')
          }
        }
       )
     }
    
    } 
  
}

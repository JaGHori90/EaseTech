import { Component, inject, OnInit } from '@angular/core';
import { UserService } from '../../shared/services/user.service';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CallApiService } from '../../shared/services/call-api.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-video-call',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './video-call.component.html',
  styleUrl: './video-call.component.css'
})
export class VideoCallComponent implements OnInit {

  private userProfile = inject(UserService);
  private callService = inject(CallApiService);
  private formBuilder = inject(FormBuilder);
  private toaster= inject(ToastrService);

  userId: string = '';
  isSubmitted: boolean = false;
  userIsRegistered: boolean = false;
  form:FormGroup;

  ngOnInit() 
  {
    this.loadUserProfile();
  }
  constructor() {
     this.form = this.formBuilder.group({
          userId: [this.userId],
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

  loadUserProfile() {
    this.userProfile.getUserProfile().subscribe({
      next: (data: any) => {
        this.userId = data.id;
        this.form.patchValue({userId:this.userId})
        this.loadCall();
      },
      error: (err: any) => {
        console.log('Fehler beim Laden von User' + err.error);
      }
    })
  }

  loadCall() {
    this.callService.GetCallByUserId(this.userId).subscribe({
      next: (data: any) => {
        this.userIsRegistered=true;
      },
      error: (err: any) => {
        console.log('Error creating call' + err.error);
      }
    })
  }
  
  onSubmit() {
     this.isSubmitted = true;
     
     if (!this.userId) {
      this.toaster.error('Benutzer-ID konnte nicht geladen werden.');
      return;
    }

     if(this.userIsRegistered === false && this.form.valid)
     {
      const formData = this.form.value;
      formData.isAvailable = formData.isAvailable ==='true';
       this.callService.registerCall(formData).subscribe(
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

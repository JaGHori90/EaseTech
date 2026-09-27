import { passwordValidators } from '../../shared/utils/password-validators';
import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { FirstKeyPipe } from '../../shared/pipes/first-key.pipe';
import { AuthService } from '../../shared/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { claimReq } from '../../shared/utils/claimReq-utils';
import { RouterLink } from '@angular/router';
import { HideIfClaimsNotMetDirective } from '../../shared/directives/hide-if-claims-not-met.directive';

@Component({
  selector: 'app-register-user',
  standalone: true,
  imports: [FormsModule, CommonModule,ReactiveFormsModule,FirstKeyPipe,HideIfClaimsNotMetDirective],
  templateUrl: './register-user.component.html',
  styles: ''
})
export class RegisterUserComponent {
  public formBuilder=inject(FormBuilder);
  private service=inject(AuthService);
  private toastr=inject(ToastrService);
  private router=inject(Router);

    isSubmitted:boolean = false;
    form: FormGroup;
    claimReq=claimReq;
  
    constructor() 
    {
     this.form = this.formBuilder.group(
      {
        firstName:['',[Validators.required,Validators.minLength(2),Validators.maxLength(20),Validators.pattern(/^[\p{L}\s]+$/u)]],
        lastName:['',[Validators.required,Validators.minLength(2),Validators.maxLength(20),Validators.pattern(/^[\p{L}\s]+$/u)]],
        role:['',Validators.required],
        email:['',[Validators.required,Validators.email]],
        password:['', passwordValidators],
        confirmPassword:[''],
        gender:'',
        birthday:'0001-01-01T00:00:00',
        address:'',
        zipCode:'',
        city:'',
        phoneNumber:''
      },{validators: this.passwordMatchValidator});
    }

  
    passwordMatchValidator:ValidatorFn=(control:AbstractControl):ValidationErrors|null =>
    {
      const password = control.get('password')?.value;
      const confirmPassword = control.get('confirmPassword')?.value;
      return password && confirmPassword && password !== confirmPassword
      ? { passwordMismatch: true }
      : null;
    }
  
    hasDisplayError(controlName:string):boolean
    {
      const control = this.form.get(controlName);
      return Boolean(control?.invalid) && (this.isSubmitted || Boolean(control?.touched));
    }
    
  // Registrierung
    onSubmit() {
      this.isSubmitted = true;
      if(this.form.valid)
      {
        this.service.createUser(this.form.value).subscribe(
          {
            next: (result:any) => 
              {
              if(result)
              {
                this.form.reset();
                this.isSubmitted = false;
                this.toastr.success('Super','Die Registrierung war erfolgreich');
              }      
            },
            error: (error) => 
            {
              if(error.error.errors)
              {
                error.error.errors.forEach((x:any) => 
                  {
                  switch (x.code) 
                  {
                    case 'DuplicateUserName':
                      this.toastr.error('Der Benutzername ist bereits vergeben','Fehler');
                      break;
    
                    case 'DuplicateEmail':
                      this.toastr.error('Die E-Mail-Adresse ist bereits vergeben','Fehler');
                      break;
    
                    default:
                      this.toastr.error('Unbekannter Fehler','Fehler');
                      console.log(x);
                      break;
                  }
                });
              }
              else
              {
                console.log('error',error);
              }
              
            }
        });
      }  
    }
  

}

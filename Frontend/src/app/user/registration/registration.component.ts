import { passwordValidators } from '../../shared/utils/password-validators';
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { FirstKeyPipe } from '../../shared/pipes/first-key.pipe';
import { AuthService } from '../../shared/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-registration',
  standalone: true,
  imports: [ReactiveFormsModule,CommonModule,FirstKeyPipe,RouterLink],
  templateUrl: './registration.component.html',
  styles: ''   
})
export class RegistrationComponent  {
  
  isSubmitted:boolean = false;
  form: FormGroup;
  showPassword: boolean = false;
  
  constructor(public formBuilder: FormBuilder,private service:AuthService,private toastr:ToastrService,private router:Router) 
  {
   this.form = this.formBuilder.group(
    {
      firstName:['',[Validators.required,Validators.minLength(2),Validators.maxLength(20),Validators.pattern(/^[\p{L}\s]+$/u)]],
      lastName:['',[Validators.required,Validators.minLength(2),Validators.maxLength(20),Validators.pattern(/^[\p{L}\s]+$/u)]],
      email:['',[Validators.required,Validators.email]],
      password:['', passwordValidators],
      confirmPassword:[''],
      role:'Customer',
      gender:'',
      birthday:'1000-01-01T00:00:00',
      address:'',
      zipCode:'',
      city:'',
      phoneNumber:''
    },{validator: this.passwordMatchValidator});
  } 

  passwordMatchValidator:ValidatorFn=(control:AbstractControl):null =>
  {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    if(password && confirmPassword && password.value !== confirmPassword.value)
    {
      confirmPassword?.setErrors({passwordMismatch:true});
    }
    else
    {
      confirmPassword?.setErrors(null);
    }
    return null;
  }

  hasDisplayError(controlName:string):boolean
  {
    const control = this.form.get(controlName);
    return Boolean(control?.invalid) && (this.isSubmitted || Boolean(control?.touched));
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
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
              this.toastr.success('Super','Die Registrierung war erfolgreich');
              this.form.reset();
              this.isSubmitted = false;
              setTimeout(() => {
                this.router.navigateByUrl('/user/signin');
              }, 2000);
            }     
          },
          error: (error) => {      
          {
            if( error.error.errors)
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
              console.log('Allgemeiner Fehler ohne Fehlerbeschreibung',error);
            }
            
          }}

      });

    }  
  }

};

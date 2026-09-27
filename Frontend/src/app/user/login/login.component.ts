import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../shared/services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterLink],
  templateUrl: './login.component.html',
  styles: ''   
})
export class LoginComponent implements OnInit {
  isSubmitted: boolean = false;
  form: FormGroup;
  role: string = '';
  showPassword: boolean = false;
  
  constructor(
    public formBuilder: FormBuilder,
    private service:AuthService,
    private router:Router,
    private toastr:ToastrService) 
    {
    this.form = this.formBuilder.group({
      email: ['', Validators.required],
      password: ['', Validators.required],
    });
  }
  
  ngOnInit(): void {
    if(this.service.IsLoggedIn())
    {
      this.router.navigateByUrl('/page/home');
    }
  }

  hasDisplayError(controlName:string):boolean
  {
    const control = this.form.get(controlName);
    return Boolean(control?.invalid) && (this.isSubmitted || Boolean(control?.touched)|| Boolean(control?.dirty));
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  onSubmit() {
    this.isSubmitted = true;
    if(this.form.valid)
    {
      this.service.signin(this.form.value).subscribe({
        next: (result:any) => {
          this.service.saveTocken(result.token);
          this.role = result.role;
          this.toastr.success('Erfolgreich angemeldet','Willkommen zu EaseTech!');
          if(this.role == 'Admin' || this.role == 'Employee')
          {
            setTimeout(() => {
              this.router.navigateByUrl('/dashboard');
            }, 2000);
            
          }
          else
          {
            setTimeout(() => {
              this.router.navigateByUrl('/page/home');
            }, 2000);
            
          }
          
        },
        error: (error:any) => {
          if(error.status == 400)
          {
            this.toastr.error('Falsche Email oder Passwort','Anmeldung fehlgeschlagen');
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

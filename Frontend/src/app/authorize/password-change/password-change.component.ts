import { passwordValidators } from '../../shared/utils/password-validators';
import { Component, inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { AuthService } from '../../shared/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { FirstKeyPipe } from '../../shared/pipes/first-key.pipe';
import { UserService } from '../../shared/services/user.service';

@Component({
  selector: 'app-password-change',
  imports: [CommonModule, FormsModule, FirstKeyPipe, ReactiveFormsModule],
  templateUrl: './password-change.component.html',
  styles: ''
})
export class PasswordChangeComponent {

  public formBuilder = inject(FormBuilder);
  private service = inject(AuthService);
  private toastr = inject(ToastrService);
  private userService = inject(UserService);

  isSubmitted: boolean = false;
  form: FormGroup;
  role: string = '';
  showPassword: boolean = false;

  constructor() {
    this.form = this.formBuilder.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', passwordValidators],
      confirmPassword: ['']
    }, { validator: this.passwordMatchValidator });
  }

  hasDisplayError(controlName: string): boolean {
    const control = this.form.get(controlName);
    return Boolean(control?.invalid) && (this.isSubmitted || Boolean(control?.touched));
  }

  passwordMatchValidator: ValidatorFn = (control: AbstractControl): null => {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword?.setErrors({ passwordMismatch: true });
    }
    else {
      confirmPassword?.setErrors(null);
    }
    return null;
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  onSubmit() {
    this.isSubmitted = true;
    if (this.form.valid) {
      this.userService.changePassword(this.form.value).subscribe({
        next: (result: any) => {
          this.toastr.success('Passwort wurde erfolgreich geändert');
        },
        error: (error: any) => {
          if (error.status == 400) {
            this.toastr.error('Change Password fehlgeschlagen');
          }
          else {
            console.log('error', error.error);
          }
        }
      });
    }
  }
}


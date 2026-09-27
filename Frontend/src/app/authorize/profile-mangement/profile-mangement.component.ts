import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../shared/services/user.service';
import { AuthService } from '../../shared/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Router } from '@angular/router';
import { first } from 'rxjs';
import { HideIfClaimsNotMetDirective } from '../../shared/directives/hide-if-claims-not-met.directive';
import { claimReq } from '../../shared/utils/claimReq-utils';

@Component({
  selector: 'app-profile-mangement',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './profile-mangement.component.html',
  styles: '',
})
export class ProfileMangementComponent implements OnInit {

  private userService = inject(UserService);
  private authService = inject(AuthService);
  public formBuilder = inject(FormBuilder);
  private service = inject(AuthService);
  private toastr = inject(ToastrService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  form: FormGroup;
  isSubmitted: boolean = false;
  claimReq = claimReq;
  Id = '';
  password: string = ''
  showPassword: boolean = false;

  constructor() {
    this.form = this.formBuilder.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9]{10,15}$/)],],
      address: ['', [Validators.required, Validators.minLength(5)]],
      city: ['', [Validators.required, Validators.minLength(3)]],
      zipCode: ['', [Validators.required, Validators.minLength(4)]],
      birthday: ['', Validators.required],
      gender: ['', Validators.required],
    });
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  hasDisplayError(controlName: string): boolean {
    const control = this.form.get(controlName);
    return (
      Boolean(control?.invalid) &&
      (this.isSubmitted || Boolean(control?.touched) || Boolean(control?.dirty))
    );
  }

  ngOnInit(): void {
    this.userService.getUserProfile().subscribe({
      next: (res: any) => {
        this.Id = res.id;

        this.form.patchValue({
          firstName: res.firstName,
          lastName: res.lastName,
          phoneNumber: res.phoneNumber,
          address: res.address,
          city: res.city,
          zipCode: res.zipCode,
          birthday: res.birthday,
          gender: res.gender,
        });
      },
      error: (err: any) => {
        console.log('Error while retrieving user profile:\n', err);
      },
    });
  }

  deleteAccount() {
    const request = { password: this.password };
    
    this.userService.deleteProfile(request).subscribe(
      {
        next: (data) => {
          this.toastr.success('Ihre Konto wurde erfolgreich gelöscht, Sie werden abgemeldet')
          setTimeout(() => {
            this.authService.deleteToken();
            this.router.navigateByUrl('/page/home');
          }, 5000);
        },
        error: (err: any) => {

          this.toastr.error('Die Löschung fehlgeschlagen !, bitte Rufen Sie unsere Kundenservice an');

          console.log('Es gibt einen Fehler bei der Löschung vom Konto ', err.error);
        }
      }
    );
  }

  deleteBtnClicked() {
    this.deleteAccount();
  }

  onSubmit() {
    this.isSubmitted = true;
    if (this.form.valid) {
      this.service.updateUserById(this.Id, this.form.value).subscribe({
        next: (result: any) => {
          if (result) {
            this.isSubmitted = false;
            this.toastr.success('Die Aktualisierung war erfolgreich');
          }
        },
        error: (err: any) => {
          console.log('Error while updating user profile:\n', err);
          this.toastr.error('Die Aktualisierung war nicht erfolgreich');
        },
      });
    }
  }

}

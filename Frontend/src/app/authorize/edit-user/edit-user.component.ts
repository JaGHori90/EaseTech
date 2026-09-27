import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../shared/services/user.service';
import { AuthService } from '../../shared/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { HideIfClaimsNotMetDirective } from '../../shared/directives/hide-if-claims-not-met.directive';
import { claimReq } from '../../shared/utils/claimReq-utils';

@Component({
  selector: 'app-edit-user',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './edit-user.component.html',
  styles: ''
})
export class EditUserComponent implements OnInit {

  form: FormGroup;
  isSubmitted: boolean = false;
  claimReq = claimReq;
  CustomerId : string = '';
 
  private userService=Inject(UserService);
  public formBuilder=Inject(FormBuilder);
  private service=Inject(AuthService);
  private toastr=Inject(ToastrService);
  private route=Inject(ActivatedRoute);

  constructor() {
    this.form = this.formBuilder.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      phoneNumber: [
        '',
        [Validators.required, Validators.pattern(/^[0-9]{10,15}$/)],
      ],
      address: ['', [Validators.required, Validators.minLength(5)]],
      city: ['', [Validators.required, Validators.minLength(3)]],
      zipCode: ['', [Validators.required, Validators.minLength(4)]],
      birthday: ['', Validators.required],
      gender: ['', Validators.required],
    });
  }

  hasDisplayError(controlName: string): boolean {
    const control = this.form.get(controlName);
    return (
      Boolean(control?.invalid) &&
      (this.isSubmitted || Boolean(control?.touched) || Boolean(control?.dirty))
    );
  }

  ngOnInit(): void {
    this.route.params.subscribe((params: { id: string }) => { this.CustomerId = params.id; });

    this.userService.getProfileById(this.CustomerId).subscribe(
      {
        next: (res: any) => {

          this.form.patchValue(
            {
              firstName: res.firstName,
              lastName: res.lastName,
              phoneNumber: res.phoneNumber,
              address: res.address,
              city: res.city,
              zipCode: res.zipCode,
              birthday: res.birthday,
            });
        },
        error: (err: any) => {
          console.log('error while retrieving user profile:\n', err);
        }
      }
    );
  }


  onSubmit() {
    this.isSubmitted = true;
    if (this.form.valid) {
      this.service.updateUserById(this.CustomerId, this.form.value).subscribe({
        next: (result: any) => {
          if (result) {
            this.isSubmitted = false;
            this.toastr.success('Die Aktualisierung war erfolgreich');
          }
        },
        error: (error:any) => {
          console.log('Fehler beim Aktualisieren des Benutzers:', error);
          this.toastr.error('Der Benutzername ist bereits vergeben');        
        }
      });
    }
  }


  
}

import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { RequestService } from '../../shared/services/request.service';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule, formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { UserService } from '../../shared/services/user.service';

@Component({
  selector: 'app-edit-request',
  imports: [FormsModule, CommonModule, ReactiveFormsModule],
  templateUrl: './edit-request.component.html',
  styles: ''
})
export class EditRequestComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private requestService = inject(RequestService);
  private toastr = inject(ToastrService);
  private formBuilder = inject(FormBuilder);
  private userService = inject(UserService);

  requestId: number = 0;
  userRequest: any;
  form: FormGroup;
  isSubmitted: boolean = false;
  employeeId: string='';
  employeeName: string='';

  constructor() {
    this.form = this.formBuilder.group({
      referrer: [''],
      name: [''],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9]{10,15}$/)]],
      message: ['',],
      contactStatus: ['', [Validators.required]],
      date: [''],
      employeeId: [this.employeeId]
    });
  }

  hasDisplayError(controlName: string): boolean {
    const control = this.form.get(controlName);
    return (
      Boolean(control?.invalid) &&
      (this.isSubmitted || Boolean(control?.touched))
    );
  }

  loadUserProfile()
  {
    this.userService.getUserProfile().subscribe({
      next:(data:any)=>
      {
        this.employeeId = data.id;
        this.form.patchValue({ employeeId: this.employeeId });
        this.employeeName = data.firstName + " "+ data.lastName;
      },
      error:(err:any)=>
      {
        console.log('Fehler beim Laden von UserProfile'+err.error);
      }
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => { this.requestId = params['id']; });

    this.loadUserProfile();

    this.requestService.getRequestById(this.requestId).subscribe(
      {
        next: (res: any) => {
          this.form.patchValue({
            referrer: res.referrer,
            name: res.name,
            phoneNumber: res.phoneNumber,
            message: res.message,
            date: res.date,
            employeeId: res.employeeId,
            contactStatus: res.contactStatus
          });
        },
        error: (err: any) => {
          console.log('Beim Laden von Request ist ein Fehler aufgetreten' + err.error);
        }
      }
    );
  }

  onSubmit() {
    this.isSubmitted = true;
    if (!this.form.value.employeeId) {  
      this.loadUserProfile();
      this.form.patchValue({ employeeId: this.employeeId });  // Richtige Zuweisung
    }
    this.requestService.updateRequest(this.requestId, this.form.value).subscribe({
      next: (res: any) => {
        if (res.message) {
          this.isSubmitted = false;
          this.toastr.success('Die Anfrage wurde erfolgreich aktualisiert');
        }
      },
      error: (err: any) => {
        console.log('Fehler beim Update der Anfrage' + err.error);
        this.toastr.error('Fehler beim Update der Anfrage');
      },
    });
  }
}

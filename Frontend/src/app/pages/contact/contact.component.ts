
import { Component, inject, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from "@angular/forms";
import { AuthService } from "../../shared/services/auth.service";
import { CommonModule } from "@angular/common";
import { RequestService } from "../../shared/services/request.service";
import { ToastrService } from "ngx-toastr";
import { Router } from "@angular/router";

@Component({
    selector: 'app-contact',
    standalone: true,
    imports: [FormsModule,CommonModule,ReactiveFormsModule],
    templateUrl: './contact.component.html',
    styles: ''   
  })

  export class ContactComponent{
  
    private service=inject(AuthService);
    private requestService=inject(RequestService);
    private formBuilder=inject(FormBuilder);
    private toastr=inject(ToastrService);
    private router = inject(Router);
      
    isSubmitted: boolean = false;
    form:FormGroup

    constructor() {
      this.form= this.formBuilder.group({
        referrer:[''],
        name:[''],
        phoneNumber:['',[Validators.required,Validators.pattern(/^[0-9]{10,15}$/)]],
        message:[''],
        date:[new Date().toISOString()],
        contactStatus:['Unberührt'],
        EmployeeId:['']
      });
    }

    hasDisplayError(controlName: string): boolean {
      const control = this.form.get(controlName);
      return (
        Boolean(control?.invalid) &&
        (this.isSubmitted || Boolean(control?.touched) )
      );
    }

 

  onSubmit() {
    this.isSubmitted = true;
    if (this.form.invalid) {
      return;
    }
    this.requestService.createRequest(this.form.value).subscribe({
      next: (res: any) => {
        if(res.message){
          this.isSubmitted = false;
          this.toastr.success('Die Anfrage wurde erfolgreich erstellt');
          setTimeout(() => {
            this.router.navigateByUrl('/page/home');
          }, 5000);
        }
      },
      error: (err: any) => {
        this.toastr.error('Fehler beim Erstellen der Anfrage');
      },
    });
  }
}
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { OrderService } from '../../shared/services/order.service';
import { UserService } from '../../shared/services/user.service';
import { ServiceService } from '../../shared/services/service.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-create-order',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './create-order.component.html',
  styles: ''
})
export class CreateOrderComponent implements OnInit {
  employeeName: string = '';
  employeeId: string = '';
  form: FormGroup;
  services: any[] = [];
  customers: any[] = [];
  serviceId: number = 1;
  servicePricePerMinute: number = 0;
  date = new Date().toISOString().slice(0, 16);
  isSubmitted: boolean = false;

  private formBuilder= inject(FormBuilder);
  private orderService= inject(OrderService);
  private userService= inject(UserService);
  private service=inject(ServiceService);
  private toaster=inject(ToastrService);

  constructor() {
    this.form = this.formBuilder.group({
      serviceId: [, [Validators.required]],
      employeeId: [],
      customerId: ['', [Validators.required]],
      date: [this.date],
      timeOfService: [, [Validators.required, Validators.pattern(/^[0-9]*$/)]],
      orderStatus: ['', [Validators.required]],
      totalCost: [],
      paymentMethod: ['', [Validators.required]],
      appointment: ['']
    });

  }
 
  updateTotalCost(): void {
    const timeValue = this.form.get('timeOfService')?.value;
    const serviceId = this.form.get('serviceId')?.value;
    if (serviceId && timeValue) {
      this.loadServiceById(serviceId);
    }
  }

  loadServiceById(id: number): void {
    this.service.getServiceById(id).subscribe(
      (data: any) => {
        this.servicePricePerMinute = data.pricePerMinute;
  
        // Berechnung der Gesamtkosten nach dem Laden des Preises
        const timeValue = this.form.get('timeOfService')?.value;
        const cost = timeValue * this.servicePricePerMinute;
        const totalCost = parseFloat(cost.toFixed(2));
        this.form.patchValue({ totalCost });
      },
      (err: any) => {
        console.log('Fehler beim Laden des Service:', err.error);
      }
    );
  }

  calculateTotalCost(): void {
    const timeValue = this.form.get('timeOfService')?.value;
    const servicePrice = this.servicePricePerMinute;
    const cost = timeValue * servicePrice;
    const totalCost = parseFloat(cost.toFixed(2));
    this.form.patchValue({ totalCost });
  }

  hasDisplayError(controlName: string): boolean {
    const control = this.form.get(controlName);
    return (
      Boolean(control?.invalid) &&
      (this.isSubmitted || Boolean(control?.touched))
    );
  }

  ngOnInit(): void {
    this.loadService();
    this.loadUser();
    this.loadCustomers();

    this.form.get('serviceId')?.valueChanges.subscribe(() => {
      this.updateTotalCost();
    });
  
    this.form.get('timeOfService')?.valueChanges.subscribe(() => {
      this.updateTotalCost();
    });
  }

  loadCustomers() {
    this.userService.getAllCusotmer().subscribe(
      {
        next: (res: any) => {
          this.customers = res;
        },
        error: (err: any) => {
          console.log('Beim Laden von Kunden ist ein Fehler aufgetreten' + err.error);

        }
      }
    );
  }

  loadService() {
    this.service.getAllServices().subscribe({
      next: (res: any) => {
        this.services = res;
      },
      error: (err: any) => {
        alert('Beim Laden von Service ist ein Fehler aufgetreten' + err.error);
      }
    });
  }

  loadUser() {
    this.userService.getUserProfile().subscribe(
      {
        next: (res: any) => {
          this.employeeName = res.firstName + ' ' + res.lastName;
          this.employeeId = res.id;
          this.form.patchValue({ employeeId: this.employeeId });
        },
        error: (err: any) => {
          alert('Beim Laden von User ist ein Fehler aufgetreten' + err.error);
        }
      }
    );
  }

  onSubmit() {
    this.orderService.createOrder(this.form.value).subscribe({
      next: (res: any) => {
        if (res.message) {
          this.toaster.success("Die Bestellung wurde erfolgreich erstellt");
        }
      },
      error: (err: any) => {
        this.toaster.error('Fehler beim Erstellen der Bestellung')
        console.log('Fehler beim Erstellen der Bestellung' ,err.error.Message);
       
      },
    });
  }
}

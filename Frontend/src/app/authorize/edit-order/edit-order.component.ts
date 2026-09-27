import { Component, inject, OnInit } from '@angular/core';
import { OrderService } from '../../shared/services/order.service';
import { ActivatedRoute } from '@angular/router';
import { CommonModule, formatDate } from '@angular/common';
import { Form, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../shared/services/user.service';
import { ServiceService } from '../../shared/services/service.service';
import { ToastrService } from 'ngx-toastr';
import { InvoiceService } from '../../shared/services/invoice.service';

@Component({
  selector: 'app-edit-order',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './edit-order.component.html',
  styles: ''
})
export class EditOrderComponent implements OnInit {

  private oderService = inject(OrderService);
  private route = inject(ActivatedRoute);
  private userService = inject(UserService);
  private invoiceService = inject(InvoiceService);
  private formBuilder = inject(FormBuilder);
  private service = inject(ServiceService);
  private toastr = inject(ToastrService);


  orderId: number = 0;
  customerId: string = '';
  employeeId: string = '';
  customerName: string = '';
  orderDate: string = '';
  order: any;
  employeeOrAdmins: any[] = [];
  services: any[] = [];
  isSubmitted: boolean = false;
  form: FormGroup;
  servicePricePerMinute: number = 0;

  constructor() {
    this.form = this.formBuilder.group({
      serviceId: ['', [Validators.required]],
      employeeId: ['', [Validators.required]],
      customerId: ['', [Validators.required]],
      date: [''],
      timeOfService: [, [Validators.required, Validators.pattern(/^[0-9]*$/)]],
      orderStatus: ['', [Validators.required]],
      totalCost: [],
      paymentMethod: ['', [Validators.required]],
      appointment: ['']
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.orderId = +params['id'];
    });

    this.loadOrder();
    this.loadService();
    this.loadEmployeeOrAdmins();
    

  }

  hasDisplayError(controlName: string): boolean {
    const control = this.form.get(controlName);
    return (
      Boolean(control?.invalid) &&
      (this.isSubmitted || Boolean(control?.touched))
    );
  }

  loadOrder() {
    this.oderService.getOrderById(this.orderId).subscribe({
      next: (res: any) => {
        this.order = res;
        this.customerId = res.customerId;
        this.employeeId = res.employeeId;
        this.loadCustomer();

        this.form.patchValue({
          serviceId: res.serviceId,
          employeeId: res.employeeId,
          customerId: res.customerId,
          date: res.date,
          timeOfService: res.timeOfService,
          orderStatus: res.orderStatus,
          totalCost: res.totalCost,
          paymentMethod: res.paymentMethod,
          appointment: res.appointment
        });

      },
      error: (err: any) => {
        console.log('Beim Laden von Bestellungs ein Fehler aufgetreten' + err.message);
      }
    });
  }

  loadCustomer() {
    this.userService.getProfileById(this.customerId).subscribe({
      next: (res: any) => {
        this.customerName = res.firstName + ' ' + res.lastName;
      },
      error: (err: any) => {
        console.log('Beim Laden von Kunden ein Fehler aufgetreten' + err.message);
      }
    });
  }

  loadEmployeeOrAdmins() {
    this.userService.getAllEmployeesAndAdmins().subscribe({
      next: (res: any) => {
        this.employeeOrAdmins = res;
      },
      error: (err: any) => {
        console.log('Beim Laden von Mitarbeiter ein Fehler aufgetreten' + err.message);
      }
    });
  }

  loadService() {
    this.service.getAllServices().subscribe({
      next: (res: any) => {
        this.services = res;
      },
      error: (err: any) => {
        console.log('Beim Laden von Service ist ein Fehler aufgetreten' + err.error);
      }
    });
  }
  calculateTotalCost(): void {
    const timeValue = this.form.get('timeOfService')?.value;
    const servicePrice = this.servicePricePerMinute;
    const cost = timeValue * servicePrice;
    const totalCost = parseFloat(cost.toFixed(2));
    this.form.patchValue({ totalCost });
  }

  onSubmit() {
    this.isSubmitted = true;
    this.oderService.updateOrderById(this.orderId, this.form.value).subscribe
      (
        {
          next: (res: any) => {
            if (res.message) {
              this.isSubmitted = false;
              this.toastr.success('Die Aktualisierung war erfolgreich');

              if (this.form.value.orderStatus === 'Abgeschlossen') {
                this.invoiceService.createInvoice(this.orderId).subscribe(
                  {
                    next: (res: any) => {
                      if (res) {
                        this.toastr.success('Rechnung wurde erfolgreich erstellt');
                      }
                    },
                    error: (err: any) => {
                      console.log('Beim Erstellen von Rechnung ist ein Fehler aufgetreten' + err.message);
                      this.toastr.error('Beim Erstellen von Rechnung ist ein Fehler aufgetreten');
                    }
                  });
              }
            }
          },
          error: (err: any) => {
            console.log('Beim Aktualisieren von Bestellung ist ein Fehler aufgetreten' + err.message);
            this.toastr.error('Beim Aktualisieren ein Fehler aufgetreten');
          }
        });
  }
}

import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { InvoiceService } from '../../shared/services/invoice.service';
import { OrderService } from '../../shared/services/order.service';
import { UserService } from '../../shared/services/user.service';
import { ServiceService } from '../../shared/services/service.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-edit-invoice',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './edit-invoice.component.html',
  styles: ''
})
export class EditInvoiceComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private toaster= inject(ToastrService);
  private invoiceService = inject(InvoiceService);
  private userService = inject(UserService);
  private formBuilder = inject(FormBuilder);
  private service = inject(ServiceService);

  invoiceId: number = 0;
  isSubmitted: boolean = false;
  form: FormGroup;
  invoice: any;
  customerId: string = '';
  customerName: string = '';
  employeeId: string = '';
  employeeName: string = '';
  serviceId: number = 0;
  serviceName: string = '';

  constructor() {
    this.form = this.formBuilder.group({
      serviceId: [''],
      employeeId: [''],
      customerId: [''],
      orderDate: [''],
      invoiceDate: [''],
      orderId: [''],
      totalAmount: [],
      taxAmount: [],
      netAmount: [],
      paymentStatus: ['', [Validators.required]],
      paymentReference: [''],
      paymentMethod: ['', [Validators.required]],
    });
  }


  hasDisplayError(controlName: string): boolean {
    const control = this.form.get(controlName);
    return (
      Boolean(control?.invalid) &&
      (this.isSubmitted || Boolean(control?.touched))
    );
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.invoiceId = params['id'];
    });
    this.loadInvoice();
  }

  loadInvoice() {
    this.invoiceService.getInvoiceById(this.invoiceId).subscribe({
      next: (res: any) => {
        this.invoice = res;
        this.customerId = this.invoice.customerId;
        this.employeeId = this.invoice.employeeId;
        this.serviceId = this.invoice.serviceId;
        this.loadService();
        this.loadCustomer();
        this.loadEmployee();

        this.form.patchValue({
          invoiceId: this.invoice.invoiceId,
          serviceId: this.invoice.serviceId,
          employeeId: this.invoice.employeeId,
          customerId: this.invoice.customerId,
          invoiceDate: this.invoice.invoiceDate,
          orderDate: this.invoice.orderDate,
          orderId: this.invoice.orderId,
          orderStatus: this.invoice.orderStatus,
          totalAmount: this.invoice.totalAmount,
          taxAmount: this.invoice.taxAmount,
          netAmount: this.invoice.netAmount,
          paymentMethod: this.invoice.paymentMethod,
          paymentStatus: this.invoice.paymentStatus,
          paymentReference: this.invoice.paymentReference,
        });
      },
      error: (err: any) => {
        console.log('Beim Laden von Rechnung ein Fehler aufgetreten' + err.message);
      }
    });
  }

  loadService() {
    this.service.getServiceById(this.serviceId).subscribe({
      next: (res: any) => {
        this.serviceName = res.serviceName;
      },
      error: (err: any) => {
        console.log('Beim Laden von Service ein Fehler aufgetreten' + err.message);
      }
    });
  }

  loadCustomer() {
    this.userService.getProfileById(this.customerId).subscribe({
      next: (res: any) => {
        this.customerName = res.firstName + ' ' + res.lastName;
      },
      error: (err: any) => {
        alert('Beim Laden von Kunde ein Fehler aufgetreten' + err.message);
      }
    });
  }

  loadEmployee() {
    this.userService.getProfileById(this.employeeId).subscribe({
      next: (res: any) => {
        this.employeeName = res.firstName + ' ' + res.lastName;
      },
      error: (err: any) => {
        alert('Beim Laden von Mitarbeiter ein Fehler aufgetreten' + err.message);
      }
    });
  }

  onSubmit() {
    this.invoiceService.updateInvoiceById(this.invoiceId, this.form.value).subscribe({
      next: (res: any) => {
        this.toaster.success('Rechnung erfolgreich aktualisiert');
      },
      error: (err: any) => {
        this.toaster.warning('Beim Aktualisieren von Rechnung ein Fehler aufgetreten' + err.message);
      }
    });
  }

}

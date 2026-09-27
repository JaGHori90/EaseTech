import { Component, inject, OnInit } from '@angular/core';
import { OrderService } from '../../shared/services/order.service';
import { ActivatedRoute } from '@angular/router';
import { InvoiceService } from '../../shared/services/invoice.service';
import { UserService } from '../../shared/services/user.service';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ServiceService } from '../../shared/services/service.service';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-my-invoice',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './my-invoice.component.html',
  styleUrl: './my-invoice.component.css'
})
export class MyInvoiceComponent implements OnInit {
  private orderService = inject(OrderService);
  private route = inject(ActivatedRoute);
  private invoiceService = inject(InvoiceService);
  private userService = inject(UserService);
  private formBuilder = inject(FormBuilder);
  private service = inject(ServiceService);

  orderId: number = 0;
  invoice: any;
  order: any;
  customerId: string = '';
  customerName: string = '';
  employeeId: string = '';
  employeeName: string = '';
  serviceId: number = 0;
  serviceName: string = '';
  pricePerMinute: number = 0;
  customerAdress: string = '';
  cutomerCityAndZip: string = '';


  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.orderId = params['id'];
    });
    this.loadInvoice();
  }

  loadInvoice() {
    this.invoiceService.getInvoiceByOrderId(this.orderId).subscribe({
      next: (res: any) => {
        this.invoice = res;
        this.customerId = this.invoice.customerId;
        this.employeeId = this.invoice.employeeId;
        this.serviceId = this.invoice.serviceId;
        this.loadService();
        this.loadCustomer();
        this.loadEmployee();
        this.loadOrder();
      },
      error: (err: any) => {
        if (err.status === 404) {
          this.invoiceService.createInvoice(this.orderId).subscribe({
            next: (res: any) => {
              this.loadInvoice();
              this.loadService();
              this.loadCustomer();
              this.loadEmployee();
              this.loadOrder();
            },
            error: (err: any) => {
              console.log('Beim Erstellen von Rechnung ein Fehler aufgetreten' + err.message);
            }
          });
        }
      }
    });
  }

  loadService() {
    this.service.getServiceById(this.serviceId).subscribe({
      next: (res: any) => {
        this.serviceName = res.serviceName;
        this.pricePerMinute = res.pricePerMinute;
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
        this.customerAdress = res.address;
        this.cutomerCityAndZip = res.city + ' ' + res.zipCode;
      },
      error: (err: any) => {
        console.log('Beim Laden von Kunde ein Fehler aufgetreten' + err.message);
      }
    });
  }

  loadEmployee() {
    this.userService.getProfileById(this.employeeId).subscribe({
      next: (res: any) => {
        this.employeeName = res.firstName + ' ' + res.lastName;
      },
      error: (err: any) => {
        console.log('Beim Laden von Mitarbeiter ein Fehler aufgetreten' + err.message);
      }
    });
  }

  loadOrder() {
    this.orderService.getOrderById(this.orderId).subscribe({
      next: (res: any) => {
        this.order = res;
      },
      error: (err: any) => {
        console.log('Beim Laden von Bestellung ein Fehler aufgetreten' + err.message);
      }
    });
  }


}

import { Component, inject, OnInit } from '@angular/core';
import { InvoiceService } from '../../shared/services/invoice.service';
import { UserService } from '../../shared/services/user.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../shared/services/order.service';
import { ServiceService } from '../../shared/services/service.service';

@Component({
  selector: 'app-overview-invoice',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './overview-invoice.component.html',
  styles: ''
})
export class OverviewInvoiceComponent implements OnInit {

  private invoiceService = inject(InvoiceService);
  private userService = inject(UserService);
  private orderService = inject(OrderService);
  private service = inject(ServiceService);

  filterByEmployee: string = '';
  filterByCustomer: string = '';
  filterByService: string = '';
  filterByStatus: string = '';
  filterByReferenceNr: string = '';
  filterByPaymentReference: string = '';
  filterByPaymentMethod: string = '';

  invoices: any[] = [];
  services: any[] = [];
  acvtiveEmployee: any;

  ngOnInit(): void {
    this.loadActiveEmplyee();
    this.loadAllInvoices();
    this.loadAllServices();
}

  loadAllServices()
  {
    this.service.getAllServices().subscribe({
      next: (res: any) => {
        this.services = res;
      },
      error: (err: any) => {
        console.log('Beim Laden von Dienstleistungen ein Fehler aufgetreten' + err.message);
      }
    });
  }

  loadActiveEmplyee() {
    this.userService.getUserProfile().subscribe({
      next: (res: any) => {
        this.acvtiveEmployee = res;
      },
      error: (err: any) => {
        console.log('Beim Laden von Mitarbeiter ein Fehler aufgetreten' + err.message);
      }
    });
  }

  loadAllInvoices()
  {
    this.invoiceService.getAllInvoices().subscribe({
      next: (res: any) => {
        this.invoices = res;
      },
      error: (err: any) => {
        console.log('Beim Laden von Rechnungen ein Fehler aufgetreten' + err.message);
      }
    });
  }

  editButtonClicked(): boolean {
    if (this.acvtiveEmployee) {
      return this.acvtiveEmployee?.role === 'Admin';
    }
    return false;
  }

}

import { Component, inject, OnInit } from '@angular/core';
import { OrderService } from '../../shared/services/order.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { UserService } from '../../shared/services/user.service';
import { InvoiceService } from '../../shared/services/invoice.service';
import { ServiceService } from '../../shared/services/service.service';

@Component({
  selector: 'app-overview-order',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './overview-order.component.html',
  styles: ''
})
export class OverviewOrderComponent implements OnInit {

  orders: any[] = [];
  services: any[] = [];
  employee: any;
  activeEmployee: any;
  filterByEmployee: string = '';
  filterByCustomer: string = '';
  filterByService: string = '';
  filterByStatus: string = '';
  filterByPaymentMethod: string = '';

  private userService = inject(UserService);
  private orderService = inject(OrderService);
  private invoiceService = inject(InvoiceService);
  private service = inject(ServiceService);

  ngOnInit(): void {
    this.loadOrder();
    this.loadService();
    this.loadActiveEmplyee();
  }

  loadOrder() {
    this.orderService.getAllOrdersDto().subscribe({
      next: (res: any) => {
        this.orders = res;
      },
      error: (err: any) => {
        console.log('Beim Laden von Bestellungs ein Fehler aufgetreten' + err.message);
      }
    });
  }

  loadService() {
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
    this.userService.getUserProfile().subscribe(
      {
        next: (res: any) => {
          this.activeEmployee = res;
        },
        error: (err: any) => {
          console.log('Beim Laden von Mitarbeiter ein Fehler aufgetreten' + err.message);
        }
      }
    );
  }

  editButtonClicked(employeeId: string): boolean {
    if (this.activeEmployee) {
      return this.activeEmployee?.role === 'Admin' || this.activeEmployee?.id === employeeId;
    }
    return false;
  }
 
}





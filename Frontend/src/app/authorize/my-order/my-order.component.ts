import { Component, inject, OnInit } from '@angular/core';
import { UserService } from '../../shared/services/user.service';
import { OrderService } from '../../shared/services/order.service';
import { ServiceService } from '../../shared/services/service.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-my-order',
  imports: [FormsModule, CommonModule,RouterLink],
  templateUrl: './my-order.component.html',
  styleUrl: './my-order.component.css'
})
export class MyOrderComponent implements OnInit {

  private userService = inject(UserService);
  private orderService = inject(OrderService);
  private service = inject(ServiceService);


  userId: string = "";
  noOrder: boolean = false;
  orders: any[] = [];
  services: any[] = [];
  filterByStatus: string = "";
  filterByService: string = "";
  filterByDate: string = "";
  today: Date = new Date();
  sixMonthsAgo: Date = new Date();

  ngOnInit() {
    this.loadUser();
    this.loadService();
    this.sixMonthsAgo.setMonth(this.today.getMonth() - 6);
  }

  loadUser() {
    this.userService.getUserProfile().subscribe(
      {
        next: (res: any) => {
          this.userId = res.id;
          this.loadOrder();
        },
        error: (err: any) => {
          console.log(err);
        }
      }
    );
  }

  loadOrder() {
    this.orderService.getOrderByCustomerId(this.userId).subscribe(
      {
        next: (res: any) => {
          this.orders = res;
        },
        error: (err: any) => {
          if (err.status === 404) {
            this.noOrder = true;
          }
          console.log('Beim Laden von Bestellungen ein Fehler aufgetreten' + err.message);
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
        console.log('Beim Laden von Dienstleistungen ein Fehler aufgetreten' + err.message);
      }
    });
  }

  matchesDateFilter(orderDate: Date): boolean {
    if (!this.filterByDate) return true; // Kein Filter gesetzt → Zeige alle
    if (this.filterByDate === "last6months") return new Date(orderDate) >= this.sixMonthsAgo;
    return true;
  }

}

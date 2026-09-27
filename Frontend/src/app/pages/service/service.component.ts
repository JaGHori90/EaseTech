import { Component, inject, OnInit } from "@angular/core";
import { UserService } from "../../shared/services/user.service";
import { CallService } from "../../shared/services/call.service";
import { CallComponent } from "../../stream/call/call.component";
import { CommonModule } from "@angular/common";
import { CallApiService } from "../../shared/services/call-api.service";
import { OrderService } from "../../shared/services/order.service";

@Component({
  selector: 'app-service',
  standalone: true,
  imports: [CommonModule, CallComponent],
  templateUrl: './service.component.html',
  styles: ''
})
export class ServiceComponent implements OnInit {

  callingService = inject(CallService);
  userService = inject(UserService);
  callApiService = inject(CallApiService);
  orderService = inject(OrderService);

  callHasStarted: boolean = false;
  userId: string = '';
  empName: string = '';
  call: any;
  noCallisFree: boolean = false;
  lastOrder: any[] = [];

  ngOnInit() {
    this.loadUserProfile();
  }

  loadTocken() {
    const user: any = { userId: this.userId };
    this.callApiService.GetToken().subscribe({
      next: (data) => {
        this.call = data;
        localStorage.setItem('callData', JSON.stringify(data));
        console.log(data)
        // Lade den Mitarbeiternamen, falls empId vorhanden ist
        if (this.call && this.call.empId) {
          this.loadEmpName(this.call.empId);
        }
        // Starte den Call, sobald callId verfügbar ist
        if (this.call && this.call.callId) {
          this.callingService.setCallId(this.call.callId);
          this.callHasStarted = true;
          this.noCallisFree = false;
        }
      },
      error: (err: any) => {
        this.noCallisFree = true;
        if (err?.error?.error === '404') {     
          console.log('Keine freie Call gefunden');
        }
        console.log('Fehler beim Laden von Token');
      }
    });
  }

  loadEmpName(empId: string) {
    this.userService.getProfileById(empId).subscribe({
      next: (data: any) => {
        this.empName = data.firstName + ' ' + data.lastName;
      },
      error: (err: any) => {
        console.log('Fehler beim Laden des Benutzernamens');
      }
    });
  }

  loadLastOrder() {
    this.orderService.getOrderByCustomerId(this.userId).subscribe({
      next: (data: any[]) => {
        if (data && data.length > 0) {
          this.lastOrder = [data[0]]; 
        } else {
          this.lastOrder = [];
        }
      },
      error: (err: any) => {
        console.log('Fehler beim Laden von Bestellungen');
      }
    });
  }

  loadUserProfile() {
    this.userService.getUserProfile().subscribe({
      next: (res: any) => {
        this.userId = res.id;
        this.loadLastOrder();
        this.loadTocken(); // Token laden, nachdem die Benutzer-ID gesetzt wurde
      },
      error: (err: any) => {
        console.log('Fehler beim Abrufen des Benutzerprofils', err);
      }
    });
  }

  // Startet den Call, sofern die Benutzer-ID vorhanden ist
  startCall() {
    if (this.userId !== '') {   
      this.loadTocken();
      
    } else {
      console.log('Benutzer-ID ist leer.');
      return;
    }
  }
}

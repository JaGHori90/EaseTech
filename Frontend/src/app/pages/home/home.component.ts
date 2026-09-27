
import { AfterViewInit, Component, inject, LOCALE_ID, OnInit, signal } from "@angular/core";
import { AuthService } from "../../shared/services/auth.service";
import { CommonModule } from "@angular/common";
import { UserService } from "../../shared/services/user.service";
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { ServiceService } from "../../shared/services/service.service";
import { OrderService } from "../../shared/services/order.service";
import { ToastrService } from "ngx-toastr";

@Component({
    selector: 'app-home',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink],
    templateUrl: './home.component.html',
    styles: ''
})

export class HomeComponent implements OnInit {

    title: string = 'EaseTech';

    private authService = inject(AuthService);
    private userService = inject(UserService);
    private router = inject(Router);
    private orderService = inject(OrderService);
    private service = inject(ServiceService);
    private toaster = inject(ToastrService);

    serviceName: string = '';
    serviceId: number = 0;
    time: Date = new Date();
    salutation: string = this.whichTypeOfSalutation();
    lastName: string = '';
    fistName: string = '';
    device: string = '';

    ProblemsMenuIsVisible: boolean = false;
    notInWorkHours: boolean = false;
    isAppointmentLater: boolean = false;
    isRequistVisible: boolean = false;

    serviceDescription: string = '';
    serviceCostPerMinute: number = 0;
    serviceTotalCostAvg: number = 0;
    serviceDuration: number = 0;
    serviceTax: number = 0;
    serviceInfo: string = '';
    serviceTimeInfo: string = '';

    userId: string = '';


    ngOnInit(): void {
        this.loadUserProfile();
    }

    calculateTotalCostAndTax() {
        this.serviceTotalCostAvg = this.serviceCostPerMinute * this.serviceDuration;
        this.serviceTax = this.serviceTotalCostAvg - (this.serviceTotalCostAvg / 1.2);
    }

    loadServiceById() {
        this.service.getServiceById(this.serviceId).subscribe(
            {
                next: (data: any) => {
                    this.serviceDescription = data.serviceName;
                    this.serviceCostPerMinute = data.pricePerMinute;
                    this.serviceInfo = data.serviceDetails;
                    this.calculateTotalCostAndTax();
                },
                error: (err: any) => {
                    console.log("Beim laden von Service Fehler aufgetreten" + err.error);
                }
            }
        );
    }

    loadUserProfile() {
        this.userService.getUserProfile().subscribe(
            {
                next: (res: any) => {
                    this.lastName = res.lastName;
                    this.fistName = res.firstName;
                    this.userId = res.id;
                },
                error: (err: any) => {
                    console.log('error while retrieving user profile:\n', err);
                }
            }
        );
    }

    isLoggedIn() {
        return this.authService.IsLoggedIn();
    }

    whichTypeOfSalutation() {
        const hour = this.time.getHours();

        if (hour >= 5 && hour < 11) {
            return 'Guten Morgen';
        } else if (hour >= 11 && hour < 18) {
            return 'Guten Tag';
        } else if (hour >= 18 && hour < 22) {
            return 'Guten Abend';
        } else {
            return 'Willkommen'; // neutral in der Nacht
        }
    }


    selectedProblem(clickedItem: string) {
        if (clickedItem == 'computer') {
            this.device = 'Computer';
        }
        else if (clickedItem == 'tv') {
            this.device = 'Fernseher';
        }
        else if (clickedItem == 'printer') {
            this.device = 'Drucker';
        }
        this.ProblemsMenuIsVisible = true;
    }

    selectedSerivce(clickedItem: string) {
        if (clickedItem == 'connection') {
            this.serviceId = 7;
            this.serviceDuration = 15;
        }
        else if (clickedItem == 'repair') {
            this.serviceId = 2;
            this.serviceDuration = 15;

        }
        else if (clickedItem == 'setting') {
            this.serviceId = 4;
            this.serviceDuration = 15;
        }
        else if (clickedItem == 'others') {
            this.serviceId = 8;
            this.serviceDuration = 15;
        }
        else if (clickedItem == 'onsite') {
            this.serviceId = 5;
            this.serviceDuration = 20;
        }
        else if (clickedItem == 'consulting') {
            this.serviceId = 9;
            this.serviceDuration = 10;
        }

        this.isRequistVisible = true;
        this.loadServiceTimeInfo();
        this.loadServiceById();
    }

    loadServiceTimeInfo() {
        if (this.time.getDay() === 0 || this.time.getHours() < 8 || this.time.getHours() >= 17) {
            this.notInWorkHours = true;
            this.serviceTimeInfo = 'Leider sind wir außerhalb unserer Geschäftszeiten.';
        }
        else {
            this.notInWorkHours = false;
            this.serviceTimeInfo = 'Wir sind heute bis 18 Uhr für Sie da.';
        }

    }

    orderBtnClicked() {
        this.createOrder();
        this.router.navigateByUrl('/page/service');
    }

    createOrder() {
        class Order {
            employeeId:number |null = null;
            customerId: string = '';
            serviceId: number = 0;
            date: Date = new Date();
            timeOfService: number = 0;
            orderStatus: string = '';
            totalCost: number = 0;
            paymentMethod: string = '';
            appointment: Date = new Date();
        }
        const now = new Date();
        const localDate = new Date(now.getTime() - now.getTimezoneOffset() * 60000);

        const newOrder = new Order();
        newOrder.customerId = this.userId;
        newOrder.employeeId = null;
        newOrder.serviceId = this.serviceId;
        newOrder.date = localDate;
        newOrder.appointment = localDate;
        newOrder.orderStatus = 'Angerichtet';
        newOrder.paymentMethod = 'Rechnung';
        newOrder.timeOfService = this.serviceDuration;
        newOrder.totalCost = this.serviceTotalCostAvg;

        this.orderService.createOrder(newOrder).subscribe(
            {
                next: (data: any) => {
                    console.log('Die Buchung war erfolgreich');
                },
                error: (err: any) => {
                    console.log('Die Buchung war nicht erfolgreich' + err.error);
                    console.log(newOrder)
                }
            }
        );

    }

    btnClicked() {
        this.router.navigateByUrl('/page/contact');
    }





}
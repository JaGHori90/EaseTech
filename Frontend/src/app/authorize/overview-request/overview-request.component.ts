import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { RequestService } from '../../shared/services/request.service';
import { claimReq } from '../../shared/utils/claimReq-utils';
import { HideIfClaimsNotMetDirective } from '../../shared/directives/hide-if-claims-not-met.directive';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-overview-request',
  imports: [CommonModule, RouterLink,FormsModule,CommonModule],
  templateUrl: './overview-request.component.html',
  styles: ''
})
export class OverviewRequestComponent implements OnInit,OnDestroy {

  private requestService = inject(RequestService);

  claimReq = claimReq;
  private intervalId: any;
  lastName: string = '';
  fistName: string = '';
  UserProfiles: any;
  UserRequests: any;
  filterByName: string = '';
  filterByStatus: string = '';
  filterByEmployee: string = '';

  
  ngOnInit(): void {

    this.intervalId = setInterval(()=>{window.location.reload();}, 120000);

    this.requestService.getAllRequests().subscribe(
      {
        next: (res: any) => {
          this.UserRequests = res;
        },
        error: (err: any) => {
          console.log('Beim Laden von Userrequest ein Fehler aufgetreten:\n' + err);
        }
      }
    );

  }

  ngOnDestroy(): void {
    if(this.intervalId) {
      clearInterval(this.intervalId);
    }
  }
}

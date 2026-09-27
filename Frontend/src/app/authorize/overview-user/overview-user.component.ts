import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { UserService } from '../../shared/services/user.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-overview-user',
  imports: [CommonModule, RouterLink,FormsModule],
  templateUrl: './overview-user.component.html',
  styles: ''
})
export class OverviewUserComponent implements OnInit, OnDestroy {

  private userService = inject(UserService);
  private intervalId: any;

  UserProfiles: any;
  UserRequests: any;
  filterByFirtName: string = '';
  filterByLastName: string = '';
  filterByGender: string = '';
  filterByRole: string = '';
  role: string = '';

  ngOnInit(): void {

    this.intervalId = setInterval(()=>{window.location.reload();}, 120000);

    this.loadAllUserProfiles();
    this.loadUserProfile();
  }

  ngOnDestroy(): void {
    if(this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  loadAllUserProfiles()
  {
    this.userService.getAllUserProfiles().subscribe(
      {
        next: (res: any) => {
          this.UserProfiles = res;
        },
        error: (err: any) => {
          console.log('Beim Laden von User ein Fehler aufgetreten:\n' + err);
        }
      }
    );
  }

  loadUserProfile()
  {
    this.userService.getUserProfile().subscribe(
      {
        next: (res: any) => {
          this.role = res.role;
          if(this.role === 'Admin')
          {
            this.filterByRole = '';
          }
          else
          {
            this.filterByRole = 'Customer';
          }

        },
        error: (err: any) => {
          console.log('Beim Laden von User ein Fehler aufgetreten:\n' + err);
        }
      }
    );
  }


}

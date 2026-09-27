import { CommonModule } from "@angular/common";
import { Component, inject } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { Router } from "@angular/router";
import { RouterModule } from "@angular/router";
import { AuthService } from "../../shared/services/auth.service";
import { UserService } from "../../shared/services/user.service";


@Component({
  selector: 'app-header',
  standalone: true, // Add this if you are using Angular standalone components
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './header.component.html',
  styles: ''   
})
export class HeaderComponent {

  private router=inject(Router);
  private authService=inject(AuthService);
  private userService=inject(UserService);

  onLogout() {
    this.authService.deleteToken();
    this.router.navigateByUrl('/page/home');
  }

  isLoggedIn() {
    return this.authService.IsLoggedIn();
  }
}

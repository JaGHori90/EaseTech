import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { RouterLink } from "@angular/router";

@Component({
    selector: 'app-footer',
    standalone: true,
    imports: [ReactiveFormsModule, CommonModule],
    templateUrl: './footer.component.html',
    styles: ''   
  })

  export class FooterComponent {
  }


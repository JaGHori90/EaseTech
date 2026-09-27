import { Directive, ElementRef, inject, Input, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';

@Directive({
  selector: '[appHideIfClaimsNotMet]',
  standalone: true
})
export class HideIfClaimsNotMetDirective implements OnInit {
  @Input("appHideIfClaimsNotMet") cliamReq!: Function;

  private authService=inject(AuthService);
  private elementRef=inject(ElementRef);

  ngOnInit(): void {
    const claims =this.authService.getClaims();
    
    if(!this.cliamReq(claims))
    {
      this.elementRef.nativeElement.style.display='none';
    }
  }

}

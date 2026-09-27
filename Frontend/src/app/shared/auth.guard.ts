
import { inject } from '@angular/core';
import { AuthService } from './services/auth.service';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn =(route,state) => {
  const authService=inject(AuthService);
  const router=inject(Router);

    if (authService.IsLoggedIn()) 
    {
      const claimReq = route.data['claimReq'] as Function;
      if(claimReq)
      {
        const claims=authService.getClaims();
        if(!claimReq(claims))
        {
          router.navigateByUrl('/forbidden');
          return false;
        }
      }
      return true;
    } 
    else 
    {
      router.navigateByUrl('/user/signin');
      return false;
    }
  }


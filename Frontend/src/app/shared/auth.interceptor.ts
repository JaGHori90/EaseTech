import { HttpInterceptorFn } from '@angular/common/http';
import { inject, NgZone } from '@angular/core';
import { AuthService } from './services/auth.service';
import { tap } from 'rxjs';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  
  const authService=inject(AuthService);
  const router=inject(Router);
  const toastr=inject(ToastrService);
  const ngZone = inject(NgZone);

  if(authService.IsLoggedIn())
  {
    const clonedReq= req.clone({
      headers: req.headers.set('Authorization','Bearer '+authService.getToken())
    })
    return next(clonedReq).pipe(tap({
      error:(err:any)=>{
        if(err.status==401)
        {
          authService.deleteToken();

          if(!authService.wasTokenExpiredNotified())
          {
            authService.setTokenExpiredNotified(true);
            setTimeout(() => {
              toastr.info('Bitte melden Sie sich an','Anmeldung abgelaufen');         
            }, 1500);
            ngZone.run(()=>{router.navigateByUrl('/user/signin');});
          }
            
        }
        else if(err.status==403)
        {
          toastr.error('Oops! Sie haben keine Berechtigung für diese Aktion','Zugriff verweigert');
      }
      }
    })
  );
  }
  else
    return next(req);
};

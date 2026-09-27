import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from './services/auth.service';
import { claimReq } from './utils/claimReq-utils';

describe('authGuard', () => {
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  const run = (data: any = {}) =>
    TestBed.runInInjectionContext(() =>
      authGuard({ data } as ActivatedRouteSnapshot, {} as RouterStateSnapshot));

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', ['IsLoggedIn', 'getClaims']);
    router = jasmine.createSpyObj('Router', ['navigateByUrl']);
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    });
  });

  it('leitet nicht eingeloggte Benutzer zum Login um', () => {
    authService.IsLoggedIn.and.returnValue(false);

    expect(run()).toBeFalse();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/user/signin');
  });

  it('lässt eingeloggte Benutzer ohne Rollenanforderung durch', () => {
    authService.IsLoggedIn.and.returnValue(true);

    expect(run()).toBeTrue();
  });

  it('lässt Benutzer mit passender Rolle durch', () => {
    authService.IsLoggedIn.and.returnValue(true);
    authService.getClaims.and.returnValue({ role: 'Admin' });

    expect(run({ claimReq: claimReq.adminOnly })).toBeTrue();
  });

  it('leitet Benutzer mit falscher Rolle auf /forbidden um', () => {
    authService.IsLoggedIn.and.returnValue(true);
    authService.getClaims.and.returnValue({ role: 'Customer' });

    expect(run({ claimReq: claimReq.adminOnly })).toBeFalse();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/forbidden');
  });
});

import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

/** Baut ein (unsigniertes) JWT mit dem gegebenen Payload – reicht zum Testen des Decodings. */
export function fakeJwt(payload: object): string {
  const encode = (o: object) => btoa(JSON.stringify(o)).replace(/=+$/, '');
  return `${encode({ alg: 'HS256', typ: 'JWT' })}.${encode(payload)}.signature`;
}

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('ist ohne Token nicht eingeloggt', () => {
    expect(service.IsLoggedIn()).toBeFalse();
  });

  it('speichert und löscht den Token', () => {
    service.saveTocken('abc');
    expect(service.IsLoggedIn()).toBeTrue();
    expect(service.getToken()).toBe('abc');

    service.deleteToken();
    expect(service.IsLoggedIn()).toBeFalse();
  });

  it('liest die Claims aus dem JWT', () => {
    service.saveTocken(fakeJwt({ userID: '42', role: 'Employee' }));

    const claims = service.getClaims();

    expect(claims.userID).toBe('42');
    expect(claims.role).toBe('Employee');
  });

  it('signin sendet POST an /signin', () => {
    service.signin({ email: 'a@b.at', password: 'x' }).subscribe();

    const req = http.expectOne(environment.apiBaseUrl + '/signin');
    expect(req.request.method).toBe('POST');
    expect(req.request.body.email).toBe('a@b.at');
    req.flush({ token: 't', role: 'Customer' });
  });

  it('createUser sendet POST an /signup', () => {
    service.createUser({ email: 'neu@b.at' }).subscribe();

    const req = http.expectOne(environment.apiBaseUrl + '/signup');
    expect(req.request.method).toBe('POST');
    req.flush({});
  });
});

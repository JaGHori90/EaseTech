import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { ContactComponent } from './contact.component';
import { RequestService } from '../../shared/services/request.service';

describe('ContactComponent', () => {
  let fixture: ComponentFixture<ContactComponent>;
  let component: ContactComponent;
  let requestService: jasmine.SpyObj<RequestService>;
  let toastr: jasmine.SpyObj<ToastrService>;

  beforeEach(async () => {
    requestService = jasmine.createSpyObj('RequestService', ['createRequest']);
    toastr = jasmine.createSpyObj('ToastrService', ['success', 'error']);

    await TestBed.configureTestingModule({
      imports: [ContactComponent],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        { provide: RequestService, useValue: requestService },
        { provide: ToastrService, useValue: toastr },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ContactComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('sendet kein ungültiges Formular ab', () => {
    component.form.patchValue({ phoneNumber: '123' });

    component.onSubmit();

    expect(requestService.createRequest).not.toHaveBeenCalled();
    expect(component.hasDisplayError('phoneNumber')).toBeTrue();
  });

  it('sendet ein gültiges Formular ab und zeigt Erfolg', () => {
    requestService.createRequest.and.returnValue(of({ message: 'ok' }));
    component.form.patchValue({ name: 'Max', phoneNumber: '06641234567', message: 'Hallo' });

    component.onSubmit();

    expect(requestService.createRequest).toHaveBeenCalled();
    expect(toastr.success).toHaveBeenCalled();
  });

  it('zeigt bei einem Serverfehler eine Fehlermeldung', () => {
    requestService.createRequest.and.returnValue(throwError(() => ({ status: 500 })));
    component.form.patchValue({ phoneNumber: '06641234567' });

    component.onSubmit();

    expect(toastr.error).toHaveBeenCalled();
  });
});

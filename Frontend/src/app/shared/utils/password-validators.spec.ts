import { FormControl } from '@angular/forms';
import { passwordValidators } from './password-validators';

describe('passwordValidators', () => {
  const check = (value: string) => new FormControl(value, passwordValidators).errors;

  it('akzeptiert ein starkes Passwort', () => {
    expect(check('Sicher!2345')).toBeNull();
  });

  it('verlangt ein Passwort', () => {
    expect(check('')).toEqual(jasmine.objectContaining({ required: true }));
  });

  it('verlangt mindestens 8 Zeichen', () => {
    expect(check('Ab1!')?.['minlength']).toBeTruthy();
  });

  it('verlangt Groß-, Kleinbuchstaben, Ziffer und Sonderzeichen', () => {
    expect(check('alleklein1!')?.['pattern']).toBeTruthy();   // kein Großbuchstabe
    expect(check('ALLEGROSS1!')?.['pattern']).toBeTruthy();   // kein Kleinbuchstabe
    expect(check('OhneZiffer!')?.['pattern']).toBeTruthy();   // keine Ziffer
    expect(check('OhneSonder12')?.['pattern']).toBeTruthy();  // kein Sonderzeichen
  });
});

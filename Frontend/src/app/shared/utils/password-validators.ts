import { Validators } from '@angular/forms';

/**
 * Muss zu den Identity-Regeln im Backend passen (IdentityExtensions.ConfigureIdentityOptions):
 * mind. 8 Zeichen, Groß- und Kleinbuchstabe, Ziffer und Sonderzeichen.
 */
export const PASSWORD_MIN_LENGTH = 8;
export const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$/;

export const passwordValidators = [
  Validators.required,
  Validators.minLength(PASSWORD_MIN_LENGTH),
  Validators.pattern(PASSWORD_PATTERN),
];

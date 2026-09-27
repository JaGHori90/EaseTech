import { claimReq } from './claimReq-utils';

describe('claimReq', () => {
  const admin = { role: 'Admin' };
  const employee = { role: 'Employee' };
  const customer = { role: 'Customer' };
  const unknown = { role: 'Hacker' };

  it('adminOnly erlaubt nur Admins', () => {
    expect(claimReq.adminOnly(admin)).toBeTrue();
    expect(claimReq.adminOnly(employee)).toBeFalse();
    expect(claimReq.adminOnly(customer)).toBeFalse();
  });

  it('customerOnly erlaubt nur Kunden', () => {
    expect(claimReq.customerOnly(customer)).toBeTrue();
    expect(claimReq.customerOnly(admin)).toBeFalse();
  });

  it('adminOrEmployee erlaubt Admin und Mitarbeiter', () => {
    expect(claimReq.adminOrEmployee(admin)).toBeTrue();
    expect(claimReq.adminOrEmployee(employee)).toBeTrue();
    expect(claimReq.adminOrEmployee(customer)).toBeFalse();
  });

  it('all erlaubt alle bekannten Rollen, aber keine unbekannten', () => {
    expect(claimReq.all(admin)).toBeTrue();
    expect(claimReq.all(employee)).toBeTrue();
    expect(claimReq.all(customer)).toBeTrue();
    expect(claimReq.all(unknown)).toBeFalse();
  });
});

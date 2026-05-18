import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { NEVER, of, throwError } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { CustomerListComponent } from './customer-list.component';
import { Customer } from './customer.models';
import { CustomerService } from './customer.service';

describe('CustomerListComponent', () => {
  let customerService: jasmine.SpyObj<Pick<CustomerService, 'getCustomers'>>;
  let authService: jasmine.SpyObj<Pick<AuthService, 'hasPermission'>>;

  function configure(response = of([] as Customer[])): ComponentFixture<CustomerListComponent> {
    customerService = jasmine.createSpyObj<Pick<CustomerService, 'getCustomers'>>('CustomerService', ['getCustomers']);
    authService = jasmine.createSpyObj<Pick<AuthService, 'hasPermission'>>('AuthService', ['hasPermission']);
    customerService.getCustomers.and.returnValue(response);
    authService.hasPermission.and.returnValue(false);

    TestBed.configureTestingModule({
      imports: [CustomerListComponent],
      providers: [
        provideRouter([]),
        { provide: CustomerService, useValue: customerService },
        { provide: AuthService, useValue: authService }
      ]
    });

    const fixture = TestBed.createComponent(CustomerListComponent);
    fixture.detectChanges();
    return fixture;
  }

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('shows loading state', () => {
    const fixture = configure(NEVER);

    expect(fixture.nativeElement.textContent).toContain('Loading customers...');
  });

  it('shows empty state', () => {
    const fixture = configure(of([]));

    expect(fixture.nativeElement.textContent).toContain('No customers found in your scope.');
  });

  it('shows error state', () => {
    const fixture = configure(throwError(() => new Error('boom')));

    expect(fixture.nativeElement.textContent).toContain('Failed to load customers.');
  });

  it('renders customers successfully', () => {
    const fixture = configure(of([buildCustomer()]));

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Carlos Mendez');
    expect(text).toContain('NovaBank');
    expect(text).toContain('NB-001');
  });
});

function buildCustomer(): Customer {
  return {
    id: 'customer-1',
    accountId: 'account-1',
    accountCode: 'NOVABANK',
    accountName: 'NovaBank',
    firstName: 'Carlos',
    lastName: 'Mendez',
    email: 'carlos.mendez@fictional.test',
    phoneNumber: '+52-55-1234-5678',
    externalReference: 'NB-001',
    isActive: true,
    createdAt: '2026-05-18T00:00:00Z'
  };
}

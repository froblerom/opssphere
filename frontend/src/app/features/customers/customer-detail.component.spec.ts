import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { CustomerDetailComponent } from './customer-detail.component';
import { Customer, CustomerTicketSummary } from './customer.models';
import { CustomerService } from './customer.service';

describe('CustomerDetailComponent', () => {
  let customerService: jasmine.SpyObj<Pick<CustomerService, 'getCustomer' | 'getCustomerTickets' | 'deactivateCustomer'>>;
  let authService: jasmine.SpyObj<Pick<AuthService, 'hasPermission'>>;

  function configure(
    customerResponse = of(buildCustomer()),
    ticketResponse = of([] as CustomerTicketSummary[])
  ): ComponentFixture<CustomerDetailComponent> {
    customerService = jasmine.createSpyObj<Pick<CustomerService, 'getCustomer' | 'getCustomerTickets' | 'deactivateCustomer'>>(
      'CustomerService',
      ['getCustomer', 'getCustomerTickets', 'deactivateCustomer']
    );
    authService = jasmine.createSpyObj<Pick<AuthService, 'hasPermission'>>('AuthService', ['hasPermission']);
    customerService.getCustomer.and.returnValue(customerResponse);
    customerService.getCustomerTickets.and.returnValue(ticketResponse);
    customerService.deactivateCustomer.and.returnValue(of({}));
    authService.hasPermission.and.returnValue(true);

    TestBed.configureTestingModule({
      imports: [CustomerDetailComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ id: 'customer-1' })
            }
          }
        },
        { provide: CustomerService, useValue: customerService },
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: jasmine.createSpyObj<Pick<Router, 'navigate'>>('Router', ['navigate']) }
      ]
    });

    const fixture = TestBed.createComponent(CustomerDetailComponent);
    fixture.detectChanges();
    return fixture;
  }

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('loads and renders the customer', () => {
    const fixture = configure();

    const text = fixture.nativeElement.textContent as string;
    expect(customerService.getCustomer).toHaveBeenCalledOnceWith('customer-1');
    expect(text).toContain('Carlos Mendez');
    expect(text).toContain('NovaBank');
  });

  it('renders the ticket history panel', () => {
    const fixture = configure(of(buildCustomer()), of([{
      id: 'ticket-1',
      ticketNumber: 'OPS-000001',
      status: 'Open',
      priority: 'Normal',
      slaState: 'WithinSla',
      createdAt: '2026-05-18T00:00:00Z'
    }]));

    const text = fixture.nativeElement.textContent as string;
    expect(customerService.getCustomerTickets).toHaveBeenCalledOnceWith('customer-1');
    expect(text).toContain('Ticket History');
    expect(text).toContain('OPS-000001');
  });

  it('shows a safe customer load error', () => {
    const fixture = configure(throwError(() => new Error('raw failure')));

    expect(fixture.nativeElement.textContent).toContain('Customer not found.');
  });

  it('shows a safe ticket history error', () => {
    const fixture = configure(of(buildCustomer()), throwError(() => new Error('raw failure')));

    expect(fixture.nativeElement.textContent).toContain('Failed to load ticket history.');
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

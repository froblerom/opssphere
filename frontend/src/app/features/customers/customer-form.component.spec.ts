import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { Account } from '../organization/organization.models';
import { OrganizationService } from '../organization/organization.service';
import { CustomerFormComponent } from './customer-form.component';
import { Customer } from './customer.models';
import { CustomerService } from './customer.service';

describe('CustomerFormComponent', () => {
  let customerService: jasmine.SpyObj<Pick<CustomerService, 'getCustomer' | 'createCustomer' | 'updateCustomer'>>;
  let organizationService: jasmine.SpyObj<Pick<OrganizationService, 'getAccounts'>>;
  let router: jasmine.SpyObj<Pick<Router, 'navigate'>>;

  function configure(id?: string): ComponentFixture<CustomerFormComponent> {
    customerService = jasmine.createSpyObj<Pick<CustomerService, 'getCustomer' | 'createCustomer' | 'updateCustomer'>>(
      'CustomerService',
      ['getCustomer', 'createCustomer', 'updateCustomer']
    );
    organizationService = jasmine.createSpyObj<Pick<OrganizationService, 'getAccounts'>>('OrganizationService', ['getAccounts']);
    router = jasmine.createSpyObj<Pick<Router, 'navigate'>>('Router', ['navigate']);
    customerService.getCustomer.and.returnValue(of(buildCustomer()));
    organizationService.getAccounts.and.returnValue(of([buildAccount()]));

    TestBed.configureTestingModule({
      imports: [CustomerFormComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap(id ? { id } : {})
            }
          }
        },
        { provide: Router, useValue: router },
        { provide: CustomerService, useValue: customerService },
        { provide: OrganizationService, useValue: organizationService },
        {
          provide: ApiErrorParserService,
          useValue: { parse: () => ({ code: 'server_error', message: 'Request failed.', details: [] }) }
        }
      ]
    });

    const fixture = TestBed.createComponent(CustomerFormComponent);
    fixture.detectChanges();
    return fixture;
  }

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('validates required fields and email format', () => {
    const fixture = configure();
    const component = fixture.componentInstance;

    component.form.patchValue({ email: 'not-an-email' });
    component.submit();

    expect(component.form.invalid).toBeTrue();
    expect(component.form.controls.accountId.hasError('required')).toBeTrue();
    expect(component.form.controls.firstName.hasError('required')).toBeTrue();
    expect(component.form.controls.lastName.hasError('required')).toBeTrue();
    expect(component.form.controls.email.hasError('email')).toBeTrue();
    expect(customerService.createCustomer).not.toHaveBeenCalled();
  });

  it('submits create customer', () => {
    const fixture = configure();
    const component = fixture.componentInstance;
    customerService.createCustomer.and.returnValue(of(buildCustomer()));

    component.form.setValue({
      accountId: 'account-1',
      firstName: 'Carlos',
      lastName: 'Mendez',
      email: 'carlos.mendez@fictional.test',
      phoneNumber: '+52-55-1234-5678',
      externalReference: 'NB-001'
    });
    component.submit();

    expect(customerService.createCustomer).toHaveBeenCalledOnceWith({
      accountId: 'account-1',
      firstName: 'Carlos',
      lastName: 'Mendez',
      email: 'carlos.mendez@fictional.test',
      phoneNumber: '+52-55-1234-5678',
      externalReference: 'NB-001'
    });
    expect(router.navigate).toHaveBeenCalledOnceWith(['/customers', 'customer-1']);
  });

  it('loads and submits update customer', () => {
    const fixture = configure('customer-1');
    const component = fixture.componentInstance;
    customerService.updateCustomer.and.returnValue(of(buildCustomer({ lastName: 'Santos' })));

    component.form.patchValue({ lastName: 'Santos' });
    component.submit();

    expect(customerService.getCustomer).toHaveBeenCalledOnceWith('customer-1');
    expect(customerService.updateCustomer).toHaveBeenCalledOnceWith('customer-1', jasmine.objectContaining({
      lastName: 'Santos'
    }));
  });
});

function buildCustomer(overrides: Partial<Customer> = {}): Customer {
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
    createdAt: '2026-05-18T00:00:00Z',
    ...overrides
  };
}

function buildAccount(): Account {
  return {
    id: 'account-1',
    code: 'NOVABANK',
    name: 'NovaBank',
    countryId: 'country-1',
    countryCode: 'MX',
    countryName: 'Mexico',
    regionId: 'region-1',
    regionCode: 'LATAM',
    description: null,
    isActive: true,
    createdAt: '2026-05-18T00:00:00Z'
  };
}

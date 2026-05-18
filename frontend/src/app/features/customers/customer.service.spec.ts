import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { ApiClientService } from '../../core/services/api-client.service';
import { CustomerService } from './customer.service';

describe('CustomerService', () => {
  let service: CustomerService;
  let apiClient: jasmine.SpyObj<Pick<ApiClientService, 'get' | 'post' | 'put'>>;

  beforeEach(() => {
    apiClient = jasmine.createSpyObj<Pick<ApiClientService, 'get' | 'post' | 'put'>>('ApiClientService', ['get', 'post', 'put']);

    TestBed.configureTestingModule({
      providers: [
        CustomerService,
        { provide: ApiClientService, useValue: apiClient }
      ]
    });

    service = TestBed.inject(CustomerService);
  });

  it('gets customers', (done) => {
    const customers = [buildCustomer()];
    apiClient.get.and.returnValue(of({ data: customers }));

    service.getCustomers().subscribe((result) => {
      expect(result).toEqual(customers);
      expect(apiClient.get).toHaveBeenCalledOnceWith('customers');
      done();
    });
  });

  it('gets a customer', (done) => {
    const customer = buildCustomer();
    apiClient.get.and.returnValue(of({ data: customer }));

    service.getCustomer('customer-1').subscribe((result) => {
      expect(result).toEqual(customer);
      expect(apiClient.get).toHaveBeenCalledOnceWith('customers/customer-1');
      done();
    });
  });

  it('creates a customer', (done) => {
    const request = { accountId: 'account-1', firstName: 'Carlos', lastName: 'Mendez' };
    const customer = buildCustomer();
    apiClient.post.and.returnValue(of({ data: customer }));

    service.createCustomer(request).subscribe((result) => {
      expect(result).toEqual(customer);
      expect(apiClient.post).toHaveBeenCalledOnceWith('customers', request);
      done();
    });
  });

  it('updates a customer', (done) => {
    const request = { accountId: 'account-1', firstName: 'Carlos', lastName: 'Mendez' };
    const customer = buildCustomer();
    apiClient.put.and.returnValue(of({ data: customer }));

    service.updateCustomer('customer-1', request).subscribe((result) => {
      expect(result).toEqual(customer);
      expect(apiClient.put).toHaveBeenCalledOnceWith('customers/customer-1', request);
      done();
    });
  });

  it('deactivates a customer', () => {
    apiClient.post.and.returnValue(of({}));

    service.deactivateCustomer('customer-1').subscribe();

    expect(apiClient.post).toHaveBeenCalledOnceWith('customers/customer-1/deactivate', {});
  });

  it('gets customer tickets', (done) => {
    const tickets = [{ id: 'ticket-1', ticketNumber: 'OPS-000001', status: 'Open', priority: 'Normal', slaState: 'WithinSla', createdAt: '2026-05-18T00:00:00Z' }];
    apiClient.get.and.returnValue(of({ data: tickets }));

    service.getCustomerTickets('customer-1').subscribe((result) => {
      expect(result).toEqual(tickets);
      expect(apiClient.get).toHaveBeenCalledOnceWith('customers/customer-1/tickets');
      done();
    });
  });
});

function buildCustomer() {
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

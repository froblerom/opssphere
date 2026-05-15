import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { CustomerService } from '../customers/customer.service';
import { OrganizationService } from '../organization/organization.service';
import { TicketService } from './ticket.service';
import { TicketCreateComponent } from './ticket-create.component';

describe('TicketCreateComponent', () => {
  let ticketService: jasmine.SpyObj<Pick<TicketService, 'createTicket'>>;
  let customerService: jasmine.SpyObj<Pick<CustomerService, 'getCustomers'>>;
  let organizationService: jasmine.SpyObj<Pick<OrganizationService, 'getAccounts' | 'getCampaigns'>>;
  let navigateSpy: jasmine.Spy;
  let fixture: ComponentFixture<TicketCreateComponent>;

  beforeEach(() => {
    ticketService = jasmine.createSpyObj<Pick<TicketService, 'createTicket'>>('TicketService', ['createTicket']);
    customerService = jasmine.createSpyObj<Pick<CustomerService, 'getCustomers'>>('CustomerService', ['getCustomers']);
    organizationService = jasmine.createSpyObj<Pick<OrganizationService, 'getAccounts' | 'getCampaigns'>>('OrganizationService', ['getAccounts', 'getCampaigns']);

    customerService.getCustomers.and.returnValue(of([]));
    organizationService.getAccounts.and.returnValue(of([]));
    organizationService.getCampaigns.and.returnValue(of([]));

    TestBed.configureTestingModule({
      imports: [TicketCreateComponent],
      providers: [
        provideNoopAnimations(),
        provideRouter([]),
        { provide: TicketService, useValue: ticketService },
        { provide: CustomerService, useValue: customerService },
        { provide: OrganizationService, useValue: organizationService },
        {
          provide: ApiErrorParserService,
          useValue: {
            parse: () => ({ message: 'Request failed.', details: [] })
          }
        }
      ]
    });

    fixture = TestBed.createComponent(TicketCreateComponent);
    navigateSpy = spyOn(TestBed.inject(Router), 'navigate');
    fixture.detectChanges();
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('requires all mandatory fields before submitting', () => {
    const component = fixture.componentInstance as any;

    component.submit();

    expect(component.form.invalid).toBeTrue();
    expect(component.form.controls.customerId.hasError('required')).toBeTrue();
    expect(component.form.controls.accountId.hasError('required')).toBeTrue();
    expect(component.form.controls.campaignId.hasError('required')).toBeTrue();
    expect(component.form.controls.category.hasError('required')).toBeTrue();
    expect(component.form.controls.priority.hasError('required')).toBeTrue();
    expect(component.form.controls.subject.hasError('required')).toBeTrue();
    expect(component.form.controls.description.hasError('required')).toBeTrue();
    expect(ticketService.createTicket).not.toHaveBeenCalled();
  });

  it('creates a ticket and navigates to detail on success', () => {
    const component = fixture.componentInstance as any;
    ticketService.createTicket.and.returnValue(of({
      id: 'ticket-1',
      ticketNumber: 'OPS-20260515-000001',
      status: 'Open',
      priority: 'Normal',
      slaState: 'WithinSla',
      slaDueAt: null
    }));

    component.form.setValue({
      customerId: 'customer-1',
      accountId: 'account-1',
      campaignId: 'campaign-1',
      category: 'Billing',
      priority: 'Normal',
      subject: 'Invoice issue',
      description: 'My invoice is wrong.'
    });
    component.submit();

    expect(ticketService.createTicket).toHaveBeenCalledOnceWith({
      customerId: 'customer-1',
      accountId: 'account-1',
      campaignId: 'campaign-1',
      category: 'Billing',
      priority: 'Normal',
      subject: 'Invoice issue',
      description: 'My invoice is wrong.'
    });
    expect(navigateSpy).toHaveBeenCalledOnceWith(['/tickets', 'ticket-1']);
  });
});

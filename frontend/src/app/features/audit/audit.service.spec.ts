import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { ApiClientService } from '../../core/services/api-client.service';
import { AuditService } from './audit.service';

describe('AuditService', () => {
  let service: AuditService;
  let apiClient: jasmine.SpyObj<Pick<ApiClientService, 'get'>>;

  beforeEach(() => {
    apiClient = jasmine.createSpyObj<Pick<ApiClientService, 'get'>>('ApiClientService', ['get']);

    TestBed.configureTestingModule({
      providers: [
        AuditService,
        { provide: ApiClientService, useValue: apiClient }
      ]
    });

    service = TestBed.inject(AuditService);
  });

  it('getAuditLogs calls correct endpoint and unwraps paged data', (done) => {
    const item = {
      id: 'audit-1',
      actorUserId: 'user-1',
      actorDisplayName: 'Nova Supervisor',
      action: 'TicketCreated',
      entityType: 'Ticket',
      entityId: 'ticket-1',
      createdAt: '2026-05-18T00:00:00Z',
      correlationId: 'corr-1'
    };
    apiClient.get.and.returnValue(of({ data: [item], page: 1, pageSize: 25, totalCount: 1, totalPages: 1 }));

    service.getAuditLogs({ actorUserId: 'user-1', action: 'TicketCreated', page: 1, pageSize: 25 }).subscribe((result) => {
      expect(result.items).toEqual([item]);
      expect(result.totalCount).toBe(1);
      expect(apiClient.get).toHaveBeenCalledOnceWith('audit-logs?actorUserId=user-1&action=TicketCreated&page=1&pageSize=25');
      done();
    });
  });

  it('getAuditLogById calls correct endpoint and unwraps data', (done) => {
    const detail = {
      id: 'audit-1',
      actorUserId: 'user-1',
      actorDisplayName: 'Nova Supervisor',
      action: 'TicketCreated',
      entityType: 'Ticket',
      entityId: 'ticket-1',
      createdAt: '2026-05-18T00:00:00Z',
      correlationId: 'corr-1',
      previousValue: null,
      newValue: '{}'
    };
    apiClient.get.and.returnValue(of({ data: detail }));

    service.getAuditLogById('audit-1').subscribe((result) => {
      expect(result).toEqual(detail);
      expect(apiClient.get).toHaveBeenCalledOnceWith('audit-logs/audit-1');
      done();
    });
  });

  it('getEntityAuditHistory calls correct endpoint', (done) => {
    apiClient.get.and.returnValue(of({ data: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 }));

    service.getEntityAuditHistory('Ticket', 'ticket-1').subscribe((result) => {
      expect(result.items).toEqual([]);
      expect(apiClient.get).toHaveBeenCalledOnceWith('audit-logs/entity/Ticket/ticket-1');
      done();
    });
  });
});

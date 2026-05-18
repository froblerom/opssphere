import { ComponentFixture, TestBed } from '@angular/core/testing';
import { defer, of } from 'rxjs';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { AuditListComponent } from './audit-list.component';
import { AuditService } from './audit.service';

describe('AuditListComponent', () => {
  let auditService: jasmine.SpyObj<Pick<AuditService, 'getAuditLogs' | 'getAuditLogById'>>;
  let errorParser: jasmine.SpyObj<Pick<ApiErrorParserService, 'parse'>>;
  let fixture: ComponentFixture<AuditListComponent>;

  beforeEach(() => {
    auditService = jasmine.createSpyObj<Pick<AuditService, 'getAuditLogs' | 'getAuditLogById'>>(
      'AuditService',
      ['getAuditLogs', 'getAuditLogById']
    );
    errorParser = jasmine.createSpyObj<Pick<ApiErrorParserService, 'parse'>>('ApiErrorParserService', ['parse']);
    errorParser.parse.and.returnValue({ code: 'bad_request', message: 'Request failed.', details: [] });
    auditService.getAuditLogs.and.returnValue(of({
      items: [
        {
          id: 'audit-1',
          actorUserId: 'user-1',
          actorDisplayName: 'Nova Supervisor',
          action: 'TicketCreated',
          entityType: 'Ticket',
          entityId: 'ticket-1',
          createdAt: '2026-05-18T00:00:00Z',
          correlationId: 'corr-1'
        }
      ],
      page: 1,
      pageSize: 25,
      totalCount: 1,
      totalPages: 1
    }));
    auditService.getAuditLogById.and.returnValue(of({
      id: 'audit-1',
      actorUserId: 'user-1',
      actorDisplayName: 'Nova Supervisor',
      action: 'TicketCreated',
      entityType: 'Ticket',
      entityId: 'ticket-1',
      createdAt: '2026-05-18T00:00:00Z',
      correlationId: 'corr-1',
      previousValue: null,
      newValue: '{"status":"Open"}'
    }));

    TestBed.configureTestingModule({
      imports: [AuditListComponent],
      providers: [
        { provide: AuditService, useValue: auditService },
        { provide: ApiErrorParserService, useValue: errorParser }
      ]
    });
  });

  it('renders audit entries', () => {
    fixture = TestBed.createComponent(AuditListComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('TicketCreated');
    expect(text).toContain('Nova Supervisor');
    expect(text).toContain('Ticket');
  });

  it('filter by actor calls service with actor filter', () => {
    fixture = TestBed.createComponent(AuditListComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;
    component.filterForm.patchValue({ actorUserId: 'user-1' });
    component.applyFilters();

    expect(auditService.getAuditLogs).toHaveBeenCalledWith(jasmine.objectContaining({
      actorUserId: 'user-1',
      page: 1,
      pageSize: 25
    }));
  });

  it('loads detail for expanded row', async () => {
    fixture = TestBed.createComponent(AuditListComponent);
    fixture.detectChanges();
    auditService.getAuditLogById.and.returnValue(defer(() => Promise.resolve({
      id: 'audit-1',
      actorUserId: 'user-1',
      actorDisplayName: 'Nova Supervisor',
      action: 'TicketCreated',
      entityType: 'Ticket',
      entityId: 'ticket-1',
      createdAt: '2026-05-18T00:00:00Z',
      correlationId: 'corr-1',
      previousValue: null,
      newValue: '{"status":"Open"}'
    })));

    fixture.componentInstance.toggleDetail(fixture.componentInstance.auditLogs[0]);
    await fixture.whenStable();

    expect(auditService.getAuditLogById).toHaveBeenCalledOnceWith('audit-1');
    expect(fixture.componentInstance.expandedId).toBe('audit-1');
    expect(fixture.componentInstance.selectedDetail?.correlationId).toBe('corr-1');
    expect(fixture.componentInstance.formatAuditValue(fixture.componentInstance.selectedDetail?.newValue)).toContain('"status": "Open"');
  });
});

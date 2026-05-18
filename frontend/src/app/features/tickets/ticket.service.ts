import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs';

import { ApiResponse } from '../../core/auth/auth.models';
import { ApiClientService } from '../../core/services/api-client.service';
import {
  AddCommentRequest,
  AddCommentResponse,
  AssignTicketRequest,
  AssignTicketResponse,
  CloseTicketResponse,
  CreateTicketRequest,
  CreateTicketResponse,
  EligibleAgentDto,
  EscalateTicketRequest,
  EscalateTicketResponse,
  EscalationQueueItemDto,
  ResolveTicketRequest,
  ResolveTicketResponse,
  SlaSummary,
  TicketFilters,
  TicketCommentDto,
  TicketDetail,
  TicketListItem,
  TicketStatusHistoryItemDto,
  UpdateTicketPriorityRequest,
  UpdateTicketPriorityResponse,
  UpdateTicketStatusRequest,
  UpdateTicketStatusResponse
} from './ticket.models';

@Injectable({
  providedIn: 'root'
})
export class TicketService {
  private readonly apiClient = inject(ApiClientService);

  getTickets(filters: TicketFilters = {}) {
    return this.apiClient.get<ApiResponse<TicketListItem[]>>(`tickets${this.buildTicketQuery(filters)}`).pipe(map((r) => r.data));
  }

  getSlaSummary() {
    return this.apiClient.get<ApiResponse<SlaSummary>>('sla/summary').pipe(map((r) => r.data));
  }

  getTicket(id: string) {
    return this.apiClient.get<ApiResponse<TicketDetail>>(`tickets/${id}`).pipe(map((r) => r.data));
  }

  createTicket(request: CreateTicketRequest) {
    return this.apiClient.post<CreateTicketRequest, ApiResponse<CreateTicketResponse>>('tickets', request).pipe(map((r) => r.data));
  }

  getEligibleAgents(ticketId: string) {
    return this.apiClient.get<ApiResponse<EligibleAgentDto[]>>(`tickets/${ticketId}/eligible-agents`).pipe(map((r) => r.data));
  }

  getComments(ticketId: string) {
    return this.apiClient.get<ApiResponse<TicketCommentDto[]>>(`tickets/${ticketId}/comments`).pipe(map((r) => r.data));
  }

  addComment(ticketId: string, body: string) {
    const request: AddCommentRequest = {
      body: body.trim()
    };

    return this.apiClient
      .post<AddCommentRequest, ApiResponse<AddCommentResponse>>(`tickets/${ticketId}/comments`, request)
      .pipe(map((r) => r.data));
  }

  escalateTicket(ticketId: string, escalationReason: string) {
    const request: EscalateTicketRequest = {
      escalationReason: escalationReason.trim()
    };

    return this.apiClient
      .post<EscalateTicketRequest, ApiResponse<EscalateTicketResponse>>(`tickets/${ticketId}/escalate`, request)
      .pipe(map((r) => r.data));
  }

  getEscalationQueue() {
    return this.apiClient.get<ApiResponse<EscalationQueueItemDto[]>>('tickets/escalations').pipe(map((r) => r.data));
  }

  assignTicket(ticketId: string, targetAgentUserId: string, reassignmentReason?: string | null) {
    const request: AssignTicketRequest = {
      targetAgentUserId,
      reassignmentReason: reassignmentReason?.trim() || null
    };

    return this.apiClient
      .post<AssignTicketRequest, ApiResponse<AssignTicketResponse>>(`tickets/${ticketId}/assign`, request)
      .pipe(map((r) => r.data));
  }

  updateTicketStatus(ticketId: string, status: string, changeReason?: string | null) {
    const request: UpdateTicketStatusRequest = {
      status,
      changeReason: changeReason?.trim() || null
    };

    return this.apiClient
      .put<UpdateTicketStatusRequest, ApiResponse<UpdateTicketStatusResponse>>(`tickets/${ticketId}/status`, request)
      .pipe(map((r) => r.data));
  }

  updateTicketPriority(ticketId: string, priority: string, changeReason?: string | null) {
    const request: UpdateTicketPriorityRequest = {
      priority,
      changeReason: changeReason?.trim() || null
    };

    return this.apiClient
      .put<UpdateTicketPriorityRequest, ApiResponse<UpdateTicketPriorityResponse>>(`tickets/${ticketId}/priority`, request)
      .pipe(map((r) => r.data));
  }

  resolveTicket(ticketId: string, resolutionSummary: string, resolutionCode?: string | null) {
    const request: ResolveTicketRequest = {
      resolutionSummary: resolutionSummary.trim(),
      resolutionCode: resolutionCode?.trim() || null
    };

    return this.apiClient
      .post<ResolveTicketRequest, ApiResponse<ResolveTicketResponse>>(`tickets/${ticketId}/resolve`, request)
      .pipe(map((r) => r.data));
  }

  closeTicket(ticketId: string) {
    return this.apiClient
      .post<Record<string, never>, ApiResponse<CloseTicketResponse>>(`tickets/${ticketId}/close`, {})
      .pipe(map((r) => r.data));
  }

  getTicketHistory(ticketId: string) {
    return this.apiClient
      .get<ApiResponse<TicketStatusHistoryItemDto[]>>(`tickets/${ticketId}/history`)
      .pipe(map((r) => r.data));
  }

  private buildTicketQuery(filters: TicketFilters) {
    const params = new URLSearchParams();
    if (filters.status) params.set('status', filters.status);
    if (filters.priority) params.set('priority', filters.priority);
    if (filters.slaState) params.set('slaState', filters.slaState);
    if (filters.regionId) params.set('regionId', filters.regionId);
    if (filters.countryId) params.set('countryId', filters.countryId);
    if (filters.accountId) params.set('accountId', filters.accountId);
    if (filters.campaignId) params.set('campaignId', filters.campaignId);
    if (filters.supervisorUserId) params.set('supervisorUserId', filters.supervisorUserId);
    if (filters.assignedAgentUserId) params.set('assignedAgentUserId', filters.assignedAgentUserId);
    if (filters.isEscalated !== undefined && filters.isEscalated !== null && filters.isEscalated !== '') {
      params.set('isEscalated', String(filters.isEscalated));
    }
    if (filters.dateFrom) params.set('dateFrom', filters.dateFrom);
    if (filters.dateTo) params.set('dateTo', filters.dateTo);

    const query = params.toString();
    return query ? `?${query}` : '';
  }
}

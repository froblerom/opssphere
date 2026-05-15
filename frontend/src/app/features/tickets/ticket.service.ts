import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs';

import { ApiResponse } from '../../core/auth/auth.models';
import { ApiClientService } from '../../core/services/api-client.service';
import {
  AssignTicketRequest,
  AssignTicketResponse,
  CreateTicketRequest,
  CreateTicketResponse,
  EligibleAgentDto,
  TicketDetail,
  TicketListItem
} from './ticket.models';

@Injectable({
  providedIn: 'root'
})
export class TicketService {
  private readonly apiClient = inject(ApiClientService);

  getTickets() {
    return this.apiClient.get<ApiResponse<TicketListItem[]>>('tickets').pipe(map((r) => r.data));
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

  assignTicket(ticketId: string, targetAgentUserId: string, reassignmentReason?: string | null) {
    const request: AssignTicketRequest = {
      targetAgentUserId,
      reassignmentReason: reassignmentReason?.trim() || null
    };

    return this.apiClient
      .post<AssignTicketRequest, ApiResponse<AssignTicketResponse>>(`tickets/${ticketId}/assign`, request)
      .pipe(map((r) => r.data));
  }
}

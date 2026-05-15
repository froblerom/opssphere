import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs';

import { ApiResponse } from '../../core/auth/auth.models';
import { ApiClientService } from '../../core/services/api-client.service';
import { CreateTicketRequest, CreateTicketResponse, TicketDetail, TicketListItem } from './ticket.models';

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
}

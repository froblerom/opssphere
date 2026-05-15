import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs';

import { ApiResponse } from '../../core/auth/auth.models';
import { ApiClientService } from '../../core/services/api-client.service';
import { Customer, CustomerRequest, CustomerTicketSummary } from './customer.models';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private readonly apiClient = inject(ApiClientService);

  getCustomers() {
    return this.apiClient.get<ApiResponse<Customer[]>>('customers').pipe(map((r) => r.data));
  }

  getCustomer(id: string) {
    return this.apiClient.get<ApiResponse<Customer>>(`customers/${id}`).pipe(map((r) => r.data));
  }

  createCustomer(request: CustomerRequest) {
    return this.apiClient.post<CustomerRequest, ApiResponse<Customer>>('customers', request).pipe(map((r) => r.data));
  }

  updateCustomer(id: string, request: CustomerRequest) {
    return this.apiClient.put<CustomerRequest, ApiResponse<Customer>>(`customers/${id}`, request).pipe(map((r) => r.data));
  }

  deactivateCustomer(id: string) {
    return this.apiClient.post<Record<string, never>, unknown>(`customers/${id}/deactivate`, {});
  }

  getCustomerTickets(id: string) {
    return this.apiClient.get<ApiResponse<CustomerTicketSummary[]>>(`customers/${id}/tickets`).pipe(map((r) => r.data));
  }
}

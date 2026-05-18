export interface Customer {
  id: string;
  accountId: string;
  accountCode: string;
  accountName: string;
  firstName: string;
  lastName: string;
  email?: string | null;
  phoneNumber?: string | null;
  externalReference?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CustomerRequest {
  accountId: string;
  firstName: string;
  lastName: string;
  email?: string | null;
  phoneNumber?: string | null;
  externalReference?: string | null;
}

export interface CustomerTicketSummary {
  id: string;
  ticketNumber: string;
  status: string;
  priority: string;
  slaState: string;
  createdAt: string;
  resolvedAt?: string | null;
  closedAt?: string | null;
}

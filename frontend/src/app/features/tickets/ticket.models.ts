export interface TicketListItem {
  id: string;
  ticketNumber: string;
  customerName: string;
  accountName: string;
  campaignName: string;
  priority: string;
  status: string;
  slaState: string;
  isEscalated: boolean;
  createdAt: string;
  assignedAgentUserId?: string | null;
  assignedAgentName?: string | null;
}

export interface TicketDetail extends TicketListItem {
  customerId: string;
  accountId: string;
  campaignId: string;
  category: string;
  subject: string;
  description: string;
  slaDueAt?: string | null;
  createdByUserId: string;
  updatedAt?: string | null;
}

export interface CreateTicketRequest {
  customerId: string;
  accountId: string;
  campaignId: string;
  category: string;
  priority: string;
  subject: string;
  description: string;
}

export interface CreateTicketResponse {
  id: string;
  ticketNumber: string;
  status: string;
  priority: string;
  slaState: string;
  slaDueAt?: string | null;
}

export interface EligibleAgentDto {
  userId: string;
  displayName: string;
  scopeType?: string | null;
  scopeReference?: string | null;
}

export interface AssignTicketRequest {
  targetAgentUserId: string;
  reassignmentReason?: string | null;
}

export interface AssignTicketResponse {
  ticketId: string;
  ticketNumber: string;
  assignedAgentUserId: string;
  previousAgentUserId?: string | null;
  status: string;
  message: string;
}

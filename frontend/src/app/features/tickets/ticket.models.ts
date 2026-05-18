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
  resolvedAt?: string | null;
  closedAt?: string | null;
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

export interface UpdateTicketStatusRequest {
  status: string;
  changeReason?: string | null;
}

export interface UpdateTicketStatusResponse {
  ticketId: string;
  ticketNumber: string;
  previousStatus: string;
  newStatus: string;
  message: string;
}

export interface UpdateTicketPriorityRequest {
  priority: string;
  changeReason?: string | null;
}

export interface UpdateTicketPriorityResponse {
  ticketId: string;
  ticketNumber: string;
  previousPriority: string;
  newPriority: string;
  message: string;
}

export interface TicketCommentDto {
  id: string;
  ticketId: string;
  authorUserId: string;
  authorDisplayName: string;
  body: string;
  createdAt: string;
}

export interface AddCommentRequest {
  body: string;
}

export interface AddCommentResponse {
  commentId: string;
  ticketId: string;
  ticketNumber: string;
  authorUserId: string;
  authorDisplayName: string;
  body: string;
  createdAt: string;
  message: string;
}

export interface EscalateTicketRequest {
  escalationReason: string;
}

export interface EscalateTicketResponse {
  ticketId: string;
  ticketNumber: string;
  escalationId: string;
  previousStatus: string;
  newStatus: string;
  message: string;
}

export interface EscalationQueueItemDto {
  escalationId: string;
  ticketId: string;
  ticketNumber: string;
  customerName: string;
  accountName: string;
  campaignName: string;
  priority: string;
  status: string;
  slaState: string;
  escalatedAt: string;
  escalatedByUserId: string;
  escalatedByName: string;
  escalationReason: string;
}

export interface ResolveTicketRequest {
  resolutionSummary: string;
  resolutionCode?: string | null;
}

export interface ResolveTicketResponse {
  ticketId: string;
  ticketNumber: string;
  resolutionId: string;
  previousStatus: string;
  newStatus: string;
  finalSlaState: string;
  resolvedAt: string;
  message: string;
}

export interface CloseTicketResponse {
  ticketId: string;
  ticketNumber: string;
  previousStatus: string;
  newStatus: string;
  closedAt: string;
  message: string;
}

export interface TicketStatusHistoryItemDto {
  id: string;
  ticketId: string;
  previousStatus?: string | null;
  newStatus: string;
  changedByUserId: string;
  changeReason?: string | null;
  createdAt: string;
}

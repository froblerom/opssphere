export interface DashboardFilters {
  regionId?: string | null;
  countryId?: string | null;
  accountId?: string | null;
  campaignId?: string | null;
  supervisorUserId?: string | null;
  agentUserId?: string | null;
  status?: string | null;
  priority?: string | null;
  slaState?: string | null;
  dateFrom?: string | null;
  dateTo?: string | null;
}

export interface OperationalDashboard {
  generatedAtUtc: string;
  totalTicketCount: number;
  openTicketCount: number;
  assignedTicketCount: number;
  escalatedTicketCount: number;
  breachedTicketCount: number;
  atRiskTicketCount: number;
  ticketsByStatus: DashboardGroupItem[];
  ticketsByPriority: DashboardGroupItem[];
  ticketsBySlaState: DashboardGroupItem[];
  ticketsByAccount: DashboardEntityGroupItem[];
  ticketsByCampaign: DashboardEntityGroupItem[];
  ticketsByAssignedAgent: DashboardUserGroupItem[];
  ticketsBySupervisor: DashboardUserGroupItem[];
  appliedFilters: DashboardAppliedFilters;
}

export interface DashboardGroupItem {
  label: string;
  key: string;
  count: number;
  status?: string | null;
  priority?: string | null;
  slaState?: string | null;
  isEscalated?: boolean | null;
  dateFrom?: string | null;
  dateTo?: string | null;
}

export interface DashboardEntityGroupItem {
  label: string;
  entityId: string;
  count: number;
  accountId?: string | null;
  campaignId?: string | null;
  dateFrom?: string | null;
  dateTo?: string | null;
}

export interface DashboardUserGroupItem {
  label: string;
  userId?: string | null;
  count: number;
  assignedAgentUserId?: string | null;
  supervisorUserId?: string | null;
  dateFrom?: string | null;
  dateTo?: string | null;
}

export interface DashboardAppliedFilters extends DashboardFilters {}

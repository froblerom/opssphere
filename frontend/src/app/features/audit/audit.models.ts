export interface AuditLogListItem {
  id: string;
  actorUserId?: string | null;
  actorDisplayName?: string | null;
  action: string;
  entityType: string;
  entityId: string;
  createdAt: string;
  correlationId?: string | null;
}

export interface AuditLogDetail extends AuditLogListItem {
  previousValue?: string | null;
  newValue?: string | null;
}

export interface AuditLogFilters {
  actorUserId?: string | null;
  action?: string | null;
  entityType?: string | null;
  entityId?: string | null;
  fromUtc?: string | null;
  toUtc?: string | null;
  accountId?: string | null;
  campaignId?: string | null;
  page?: number | null;
  pageSize?: number | null;
}

export interface PagedApiResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

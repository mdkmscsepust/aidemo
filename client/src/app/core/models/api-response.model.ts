export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message: string | null;
  errors: Record<string, string[]> | null;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResult } from '../models/api-response.model';
import { CreateReservationDto, ReservationDto } from '../models/reservation.model';

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/reservations`;

  getMyReservations(page = 1, pageSize = 10, status?: number): Observable<ApiResponse<PaginatedResult<ReservationDto>>> {
    let url = `${this.base}?page=${page}&pageSize=${pageSize}`;
    if (status !== undefined) url += `&status=${status}`;
    return this.http.get<ApiResponse<PaginatedResult<ReservationDto>>>(url);
  }

  getById(id: string): Observable<ApiResponse<ReservationDto>> {
    return this.http.get<ApiResponse<ReservationDto>>(`${this.base}/${id}`);
  }

  create(dto: CreateReservationDto): Observable<ApiResponse<ReservationDto>> {
    return this.http.post<ApiResponse<ReservationDto>>(this.base, dto);
  }

  cancel(id: string, reason?: string): Observable<ApiResponse<ReservationDto>> {
    return this.http.put<ApiResponse<ReservationDto>>(`${this.base}/${id}/cancel`, { reason });
  }

  complete(id: string): Observable<ApiResponse<ReservationDto>> {
    return this.http.post<ApiResponse<ReservationDto>>(`${this.base}/${id}/complete`, {});
  }

  markNoShow(id: string): Observable<ApiResponse<ReservationDto>> {
    return this.http.post<ApiResponse<ReservationDto>>(`${this.base}/${id}/no-show`, {});
  }

  getForRestaurant(restaurantId: string, page = 1, pageSize = 20): Observable<ApiResponse<PaginatedResult<ReservationDto>>> {
    return this.http.get<ApiResponse<PaginatedResult<ReservationDto>>>(
      `${environment.apiUrl}/owner/restaurants/${restaurantId}/reservations?page=${page}&pageSize=${pageSize}`
    );
  }
}

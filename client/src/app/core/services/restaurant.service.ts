import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResult } from '../models/api-response.model';
import { RestaurantDetailDto, RestaurantListDto, RestaurantSearchParams } from '../models/restaurant.model';
import { AvailableSlotDto } from '../models/reservation.model';

@Injectable({ providedIn: 'root' })
export class RestaurantService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/restaurants`;

  getAll(params: RestaurantSearchParams = {}): Observable<ApiResponse<PaginatedResult<RestaurantListDto>>> {
    let httpParams = new HttpParams();
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.city) httpParams = httpParams.set('city', params.city);
    if (params.cuisine) httpParams = httpParams.set('cuisine', params.cuisine);
    if (params.priceMin != null) httpParams = httpParams.set('priceMin', params.priceMin);
    if (params.priceMax != null) httpParams = httpParams.set('priceMax', params.priceMax);
    if (params.page != null) httpParams = httpParams.set('page', params.page);
    if (params.pageSize != null) httpParams = httpParams.set('pageSize', params.pageSize);
    if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    return this.http.get<ApiResponse<PaginatedResult<RestaurantListDto>>>(this.base, { params: httpParams });
  }

  getById(id: string): Observable<ApiResponse<RestaurantDetailDto>> {
    return this.http.get<ApiResponse<RestaurantDetailDto>>(`${this.base}/${id}`);
  }

  getAvailability(id: string, date: string, partySize: number): Observable<ApiResponse<AvailableSlotDto[]>> {
    const params = new HttpParams().set('date', date).set('partySize', partySize);
    return this.http.get<ApiResponse<AvailableSlotDto[]>>(`${this.base}/${id}/availability`, { params });
  }

  create(dto: Partial<RestaurantDetailDto>): Observable<ApiResponse<RestaurantDetailDto>> {
    return this.http.post<ApiResponse<RestaurantDetailDto>>(this.base, dto);
  }

  update(id: string, dto: Partial<RestaurantDetailDto>): Observable<ApiResponse<RestaurantDetailDto>> {
    return this.http.put<ApiResponse<RestaurantDetailDto>>(`${this.base}/${id}`, dto);
  }

  approve(id: string): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.base}/${id}/approve`, {});
  }

  delete(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.base}/${id}`);
  }
}

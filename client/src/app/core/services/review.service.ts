import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResult } from '../models/api-response.model';
import { CreateReviewDto, ReviewDto } from '../models/review.model';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/reviews`;

  getForRestaurant(restaurantId: string, page = 1, pageSize = 10): Observable<ApiResponse<PaginatedResult<ReviewDto>>> {
    return this.http.get<ApiResponse<PaginatedResult<ReviewDto>>>(`${this.base}?restaurantId=${restaurantId}&page=${page}&pageSize=${pageSize}`);
  }

  create(dto: CreateReviewDto): Observable<ApiResponse<ReviewDto>> {
    return this.http.post<ApiResponse<ReviewDto>>(this.base, dto);
  }
}

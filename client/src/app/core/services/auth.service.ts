import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { StorageService } from './storage.service';
import { ApiResponse } from '../models/api-response.model';
import { AuthResponse, LoginDto, RegisterDto, UserModel } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly storage = inject(StorageService);
  private readonly base = `${environment.apiUrl}/auth`;

  private readonly _user = signal<UserModel | null>(this.loadUserFromToken());
  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => !!this._user());
  readonly isAdmin = computed(() => this._user()?.role === 'Admin');
  readonly isOwner = computed(() => this._user()?.role === 'Owner' || this._user()?.role === 'Admin');

  private loadUserFromToken(): UserModel | null {
    const token = this.storage.getAccessToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload['exp'] * 1000;
      if (Date.now() > exp) {
        this.storage.clearAll();
        return null;
      }
      return {
        id: payload['sub'] || payload['nameid'],
        email: payload['email'],
        firstName: payload['given_name'] || '',
        lastName: payload['family_name'] || '',
        role: payload['role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
      };
    } catch {
      return null;
    }
  }

  login(dto: LoginDto): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.base}/login`, dto).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.storage.setAccessToken(res.data.accessToken);
          this.storage.setRefreshToken(res.data.refreshToken);
          this._user.set(res.data.user);
        }
      })
    );
  }

  register(dto: RegisterDto): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.base}/register`, dto).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.storage.setAccessToken(res.data.accessToken);
          this.storage.setRefreshToken(res.data.refreshToken);
          this._user.set(res.data.user);
        }
      })
    );
  }

  refreshToken(): Observable<ApiResponse<AuthResponse>> {
    const token = this.storage.getRefreshToken();
    return this.http.post<ApiResponse<AuthResponse>>(`${this.base}/refresh-token`, { token }).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.storage.setAccessToken(res.data.accessToken);
          this.storage.setRefreshToken(res.data.refreshToken);
          this._user.set(res.data.user);
        }
      }),
      catchError(err => {
        this.logout();
        return throwError(() => err);
      })
    );
  }

  logout(): void {
    const token = this.storage.getRefreshToken();
    if (token) {
      this.http.post(`${this.base}/logout`, { token }).subscribe({ error: () => {} });
    }
    this.storage.clearAll();
    this._user.set(null);
    this.router.navigate(['/auth/login']);
  }
}

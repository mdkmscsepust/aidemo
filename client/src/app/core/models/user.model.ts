export interface UserModel {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phone?: string;
  role: 'Customer' | 'Owner' | 'Admin';
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserModel;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone?: string;
  role?: string;
}

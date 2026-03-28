export interface Player {
  id: number;
  firstName: string;
  lastName: string;
  age: number;
  position: string;
  clubName?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePlayerDto {
  firstName: string;
  lastName: string;
  age: number;
  position: string;
  clubName?: string;
}

export interface UpdatePlayerDto {
  firstName: string;
  lastName: string;
  age: number;
  position: string;
  clubName?: string;
}

export interface User {
  id: string;
  username: string;
  email: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  accessToken?: string;
  refreshToken?: string;
  user?: User;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message: string;
}

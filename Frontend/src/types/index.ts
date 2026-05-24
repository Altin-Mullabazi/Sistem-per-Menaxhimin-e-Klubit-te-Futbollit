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
  role?: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  accessToken?: string;
  refreshToken?: string;
  user?: User;
}

export interface Sponsor {
  id: number;
  name: string;
  logo?: string;
  website?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ClubSummary {
  id: number;
  name: string;
}

export interface SponsorDetail extends Sponsor {
  clubs: ClubSummary[];
}

export interface CreateSponsorDto {
  name: string;
  logo?: string;
  website?: string;
}

export interface UpdateSponsorDto {
  name: string;
  logo?: string;
  website?: string;
}

export interface Season {
  id: number;
  name: string;
  startDate: string;
  endDate: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSeasonDto {
  name: string;
  startDate: string;
  endDate: string;
  description?: string;
}

export interface UpdateSeasonDto {
  name: string;
  startDate: string;
  endDate: string;
  description?: string;
}

export interface Pagination {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface RegisterResponse {
  success: boolean;
  message: string;
  data?: {
    userId: string;
    email: string;
    firstName: string;
    lastName: string;
    fullName: string;
    createdAt: string;
    tokens?: {
      accessToken: string;
      refreshToken: string;
      expiresIn: number;
    };
  };
  errors?: Array<{
    field: string;
    message: string;
  }>;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message: string;
}

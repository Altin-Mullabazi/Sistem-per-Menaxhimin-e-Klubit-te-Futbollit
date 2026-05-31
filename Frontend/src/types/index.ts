export interface Club {
  id: number;
  name: string;
  city: string;
  logoUrl?: string;
  foundedYear: number;
  president?: string;
  budget?: number;
  playerCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface ClubDetail extends Club {
  players: ClubPlayer[];
}

export interface ClubPlayer {
  id: number;
  firstName: string;
  lastName: string;
  position: string;
  jerseyNumber: number;
}

export interface CreateClubDto {
  name: string;
  city: string;
  logoUrl?: string;
  foundedYear: number;
  president?: string;
  budget?: number;
}

export interface UpdateClubDto {
  name?: string;
  city?: string;
  logoUrl?: string;
  foundedYear?: number;
  president?: string;
  budget?: number;
}

export interface ClubListResponse {
  data: Club[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface Player {
  id: number;
  firstName: string;
  lastName: string;
  age: number;
  position: string;
  clubId?: number;
  clubName?: string;
  jerseyNumber?: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePlayerDto {
  firstName: string;
  lastName: string;
  age: number;
  position: string;
  clubId?: number;
  clubName?: string;
  jerseyNumber?: number;
}

export interface UpdatePlayerDto {
  firstName: string;
  lastName: string;
  age: number;
  position: string;
  clubId?: number;
  clubName?: string;
  jerseyNumber?: number;
}

export interface PlayerListResponse {
  data: Player[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
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

export interface DashboardSummary {
  totalClubs: number;
  totalPlayers: number;
  totalMatches: number;
  totalStaff: number;
  totalInjuries: number;
  totalContracts: number;
}

export interface UpcomingMatch {
  id: number;
  homeTeam: string;
  awayTeam: string;
  date: string;
  stadium: string;
}

export interface TopScorer {
  id?: number;
  playerName: string;
  clubName: string;
  goals: number;
  assists: number;
}

export interface InjuredPlayer {
  id: number;
  playerName: string;
  injury: string;
  startDate: string;
  endDate: string;
}

export interface ExpiringContract {
  id: number;
  playerName: string;
  clubName: string;
  endDate: string;
}

export interface RecentTransfer {
  id: number;
  playerName: string;
  fromClubName: string;
  toClubName: string;
  fee: string;
}

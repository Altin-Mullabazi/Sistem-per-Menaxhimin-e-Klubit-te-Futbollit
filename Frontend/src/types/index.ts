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
  firstName?: string;
  lastName?: string;
  fullName?: string;
  createdAt?: string;
  updatedAt?: string;
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

export interface Club {
  id: number;
  name: string;
}

export interface Stadium {
  id: number;
  name: string;
  city: string;
}

export type TrainingType = 1 | 2 | 3 | 4 | 5;

export interface TrainingSession {
  id: number;
  clubId: number;
  clubName: string;
  sessionDate: string;
  duration: number;
  type: TrainingType;
  notes?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTrainingSessionDto {
  clubId: number;
  sessionDate: string;
  duration: number;
  type: TrainingType;
  notes?: string;
}

export interface UpdateTrainingSessionDto {
  sessionDate?: string;
  duration?: number;
  type?: TrainingType;
  notes?: string;
}

export interface Match {
  id: number;
  homeClubId: number;
  homeClubName: string;
  awayClubId: number;
  awayClubName: string;
  matchDate: string;
  time?: string;
  homeScore?: number;
  awayScore?: number;
  status: string;
  competitionType?: string;
  stadiumId: number;
  stadiumName: string;
  seasonId: number;
  seasonName: string;
  createdAt: string;
  updatedAt: string;
}

export interface MatchDetail extends Match {
  storeLocation?: string;
  events: MatchEvent[];
  stats: PlayerStats[];
}

export interface MatchEvent {
  id: number;
  playerId: number;
  playerName: string;
  eventType: string;
  minute: number;
  description?: string;
}

export interface PlayerStats {
  id: number;
  playerId: number;
  playerName: string;
  matchId: number;
  matchDate: string;
  goalsScored: number;
  assists: number;
  yellowCards: number;
  redCards: number;
  minutesPlayed: number;
  rating?: number;
}

export interface CreatePlayerStatsDto {
  playerId: number;
  matchId: number;
  goalsScored: number;
  assists: number;
  yellowCards: number;
  redCards: number;
  minutesPlayed: number;
}

export interface UpdatePlayerStatsDto {
  goalsScored: number;
  assists: number;
  yellowCards: number;
  redCards: number;
  minutesPlayed: number;
}

export interface TopScorer {
  playerId: number;
  playerName: string;
  clubId?: number;
  clubName?: string;
  goalsScored: number;
  assists: number;
}

export interface CreateMatchDto {
  homeClubId: number;
  awayClubId: number;
  stadiumId: number;
  matchDate: string;
  time?: string;
  seasonId: number;
  competitionType?: string;
}

export interface UpdateMatchDto {
  homeScore?: number;
  awayScore?: number;
  status?: string;
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

export interface Staff {
  id: number;
  clubId?: number;
  clubName?: string;
  userId?: string;
  userEmail?: string;
  firstName: string;
  lastName: string;
  role: string;
  specialization?: string;
  employmentDate: string;
  status?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateStaffDto {
  clubId: number;
  userId: string;
  firstName: string;
  lastName: string;
  role: string;
  specialization?: string;
  employmentDate: string;
  status?: string;
}

export interface UpdateStaffDto {
  clubId?: number;
  userId?: string;
  firstName?: string;
  lastName?: string;
  role?: string;
  specialization?: string;
  employmentDate?: string;
  status?: string;
}

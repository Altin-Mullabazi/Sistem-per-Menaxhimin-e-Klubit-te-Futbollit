import apiClient from './apiClient';
import { Player, CreatePlayerDto, UpdatePlayerDto, ApiResponse, PlayerListResponse } from '../types';

/** Backend API shape for create/update player */
interface ApiPlayerWriteDto {
  firstName: string;
  lastName: string;
  position: string;
  jerseyNumber: number;
  dateOfBirth: string;
  nationality: string;
  clubId?: number;
}

const calcAge = (dateOfBirth?: string): number => {
  if (!dateOfBirth) return 0;
  const birth = new Date(dateOfBirth);
  if (Number.isNaN(birth.getTime())) return 0;
  const today = new Date();
  let age = today.getFullYear() - birth.getFullYear();
  const monthDiff = today.getMonth() - birth.getMonth();
  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
    age--;
  }
  return age;
};

const mapPlayerFromApi = (raw: Record<string, unknown>): Player => ({
  id: Number(raw.id),
  firstName: String(raw.firstName ?? ''),
  lastName: String(raw.lastName ?? ''),
  position: String(raw.position ?? ''),
  age: calcAge(raw.dateOfBirth as string | undefined),
  clubId: raw.clubId != null ? Number(raw.clubId) : undefined,
  clubName: (raw.club as { name?: string } | undefined)?.name,
  jerseyNumber: raw.jerseyNumber != null ? Number(raw.jerseyNumber) : undefined,
  createdAt: String(raw.createdAt ?? ''),
  updatedAt: String(raw.updatedAt ?? ''),
});

const ageToDateOfBirth = (age: number): string => {
  const year = new Date().getFullYear() - age;
  return new Date(year, 5, 15).toISOString();
};

const toApiPlayerPayload = (player: CreatePlayerDto | UpdatePlayerDto): ApiPlayerWriteDto => ({
  firstName: player.firstName.trim(),
  lastName: player.lastName.trim(),
  position: player.position.trim(),
  jerseyNumber: player.jerseyNumber!,
  dateOfBirth: ageToDateOfBirth(player.age),
  nationality: 'Unknown',
  clubId: player.clubId,
});

export const getApiErrorMessage = (error: unknown, fallback: string): string => {
  const err = error as {
    message?: string;
    response?: { data?: Record<string, unknown> };
  };

  const data = err.response?.data;
  if (data) {
    if (typeof data.message === 'string') return data.message;
    const errors = data.errors as Record<string, string[]> | undefined;
    if (errors) {
      return Object.values(errors).flat().join(' ');
    }
  }

  return err.message || fallback;
};

export const playerService = {
  getAllPlayers: async (): Promise<Player[]> => {
    try {
      const response = await apiClient.get('/players?page=1&pageSize=100');
      const body = response.data;
      const items = Array.isArray(body.data) ? body.data : body.data?.data || [];
      return items.map((p: Record<string, unknown>) => mapPlayerFromApi(p));
    } catch (error: unknown) {
      throw { success: false, message: getApiErrorMessage(error, 'Failed to fetch players') };
    }
  },

  getPlayers: async (
    page: number = 1,
    pageSize: number = 10,
    search?: string,
    position?: string,
    clubId?: number
  ): Promise<PlayerListResponse> => {
    try {
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', pageSize.toString());
      if (search) params.append('search', search);
      if (position) params.append('position', position);
      if (clubId) params.append('clubId', clubId.toString());

      const response = await apiClient.get(`/players?${params.toString()}`);
      const body = response.data;
      const items = Array.isArray(body.data) ? body.data : [];
      const pagination = body.pagination;

      return {
        data: items.map((p: Record<string, unknown>) => mapPlayerFromApi(p)),
        totalCount: pagination?.totalCount ?? items.length,
        pageNumber: pagination?.currentPage ?? page,
        pageSize: pagination?.pageSize ?? pageSize,
        totalPages: pagination?.totalPages ?? 1,
      };
    } catch (error: unknown) {
      throw { success: false, message: getApiErrorMessage(error, 'Failed to fetch players') };
    }
  },

  getPlayerById: async (id: number): Promise<Player | null> => {
    try {
      const response = await apiClient.get(`/players/${id}`);
      const raw = response.data.data;
      return raw ? mapPlayerFromApi(raw) : null;
    } catch (error: unknown) {
      throw { success: false, message: getApiErrorMessage(error, 'Failed to fetch player') };
    }
  },

  createPlayer: async (player: CreatePlayerDto): Promise<ApiResponse<Player>> => {
    try {
      const response = await apiClient.post('/players', toApiPlayerPayload(player));
      return response.data;
    } catch (error: unknown) {
      throw { success: false, message: getApiErrorMessage(error, 'Failed to create player') };
    }
  },

  updatePlayer: async (id: number, player: UpdatePlayerDto): Promise<ApiResponse<Player>> => {
    try {
      const response = await apiClient.put(`/players/${id}`, toApiPlayerPayload(player));
      return response.data;
    } catch (error: unknown) {
      throw { success: false, message: getApiErrorMessage(error, 'Failed to update player') };
    }
  },

  deletePlayer: async (id: number): Promise<ApiResponse<void>> => {
    try {
      const response = await apiClient.delete(`/players/${id}`);
      return response.data;
    } catch (error: unknown) {
      throw { success: false, message: getApiErrorMessage(error, 'Failed to delete player') };
    }
  },
};

import apiClient from './apiClient';
import {
  PlayerStats,
  CreatePlayerStatsDto,
  UpdatePlayerStatsDto,
  TopScorer,
  Pagination,
  ApiResponse,
} from '../types';

interface StatsListResponse {
  success: boolean;
  data: PlayerStats[];
  pagination: Pagination;
  message: string;
}

interface SingleStatsResponse {
  success: boolean;
  data: PlayerStats;
  message: string;
}

interface TopScorerResponse {
  success: boolean;
  data: TopScorer[];
  message: string;
}

export const playerStatsService = {
  async getPlayerStats(
    page: number = 1,
    pageSize: number = 10,
    playerId?: number,
    matchId?: number,
    sortBy?: string
  ): Promise<{ stats: PlayerStats[]; pagination: Pagination }> {
    try {
      const params: any = { page, pageSize };
      if (playerId) params.playerId = playerId;
      if (matchId) params.matchId = matchId;
      if (sortBy) params.sortBy = sortBy;

      const response = await apiClient.get<StatsListResponse>('/playerstats', { params });
      if (response.data.success) {
        return {
          stats: response.data.data,
          pagination: response.data.pagination,
        };
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to fetch player stats');
    }
  },

  async getTopScorers(limit: number = 10): Promise<TopScorer[]> {
    try {
      const response = await apiClient.get<TopScorerResponse>('/playerstats/top-scorers', {
        params: { limit },
      });
      if (response.data.success) {
        return response.data.data || [];
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to fetch top scorers');
    }
  },

  async getTopAssists(limit: number = 10): Promise<TopScorer[]> {
    try {
      const response = await apiClient.get<TopScorerResponse>('/playerstats/top-assists', {
        params: { limit },
      });
      if (response.data.success) {
        return response.data.data || [];
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to fetch top assists');
    }
  },

  async createPlayerStats(createData: CreatePlayerStatsDto): Promise<PlayerStats> {
    try {
      const response = await apiClient.post<SingleStatsResponse>('/playerstats', createData);
      if (response.data.success) {
        return response.data.data;
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to create player stats');
    }
  },

  async updatePlayerStats(id: number, updateData: UpdatePlayerStatsDto): Promise<PlayerStats> {
    try {
      const response = await apiClient.put<SingleStatsResponse>(`/playerstats/${id}`, updateData);
      if (response.data.success) {
        return response.data.data;
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to update player stats');
    }
  },

  async deletePlayerStats(id: number): Promise<void> {
    try {
      const response = await apiClient.delete<ApiResponse<void>>(`/playerstats/${id}`);
      if (!response.data.success) {
        throw new Error(response.data.message);
      }
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to delete player stats');
    }
  },
};

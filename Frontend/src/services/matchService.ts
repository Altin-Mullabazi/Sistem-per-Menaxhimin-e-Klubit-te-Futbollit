import { apiClient } from './apiClient';
import { Match, MatchDetail, CreateMatchDto, UpdateMatchDto, ApiResponse, Pagination } from '../types';

interface MatchListResponse {
  success: boolean;
  data: Match[];
  pagination: Pagination;
  message: string;
}

interface MatchDetailResponse {
  success: boolean;
  data: MatchDetail;
  message: string;
}

interface SingleMatchResponse {
  success: boolean;
  data: Match;
  message: string;
}

interface CountResponse {
  success: boolean;
  data: { count: number };
  message: string;
}

export const matchService = {
  async getMatches(
    page: number = 1,
    pageSize: number = 10,
    clubId?: number,
    seasonId?: number,
    status?: string
  ): Promise<{ matches: Match[]; pagination: Pagination }> {
    try {
      const params: any = { page, pageSize };
      if (clubId) params.clubId = clubId;
      if (seasonId) params.seasonId = seasonId;
      if (status) params.status = status;

      const response = await apiClient.get<MatchListResponse>('/matches', { params });
      if (response.data.success) {
        return {
          matches: response.data.data,
          pagination: response.data.pagination,
        };
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to fetch matches');
    }
  },

  async getUpcomingMatches(days: number = 7): Promise<Match[]> {
    try {
      const response = await apiClient.get<ApiResponse<Match[]>>('/matches/upcoming', {
        params: { days },
      });
      if (response.data.success) {
        return response.data.data || [];
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to fetch upcoming matches');
    }
  },

  async getMatchCount(): Promise<number> {
    try {
      const response = await apiClient.get<CountResponse>('/matches/count');
      if (response.data.success) {
        return response.data.data.count;
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to fetch match count');
    }
  },

  async getMatchById(id: number): Promise<MatchDetail> {
    try {
      const response = await apiClient.get<MatchDetailResponse>(`/matches/${id}`);
      if (response.data.success) {
        return response.data.data;
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to fetch match details');
    }
  },

  async createMatch(createMatchDto: CreateMatchDto): Promise<Match> {
    try {
      const response = await apiClient.post<SingleMatchResponse>('/matches', createMatchDto);
      if (response.data.success) {
        return response.data.data;
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to create match');
    }
  },

  async updateMatch(id: number, updateMatchDto: UpdateMatchDto): Promise<Match> {
    try {
      const response = await apiClient.put<SingleMatchResponse>(`/matches/${id}`, updateMatchDto);
      if (response.data.success) {
        return response.data.data;
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to update match');
    }
  },

  async deleteMatch(id: number): Promise<void> {
    try {
      const response = await apiClient.delete<ApiResponse<void>>(`/matches/${id}`);
      if (!response.data.success) {
        throw new Error(response.data.message);
      }
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to delete match');
    }
  },
};

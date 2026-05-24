import apiClient from './apiClient';
import {
  Season,
  CreateSeasonDto,
  UpdateSeasonDto,
  ApiResponse,
  Pagination,
} from '../types';

interface SeasonListResult {
  data: Season[];
  pagination: Pagination;
}

export const seasonService = {
  getSeasons: async (page = 1, pageSize = 10): Promise<SeasonListResult> => {
    try {
      const response = await apiClient.get('/seasons', {
        params: { page, pageSize },
      });

      return {
        data: response.data.data || [],
        pagination: response.data.pagination || {
          page,
          pageSize,
          totalCount: 0,
          totalPages: 1,
        },
      };
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to load seasons' };
    }
  },

  createSeason: async (payload: CreateSeasonDto): Promise<ApiResponse<Season>> => {
    try {
      const response = await apiClient.post('/seasons', payload);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to create season' };
    }
  },

  updateSeason: async (id: number, payload: UpdateSeasonDto): Promise<ApiResponse<Season>> => {
    try {
      const response = await apiClient.put(`/seasons/${id}`, payload);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to update season' };
    }
  },

  deleteSeason: async (id: number): Promise<ApiResponse<void>> => {
    try {
      const response = await apiClient.delete(`/seasons/${id}`);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to delete season' };
    }
  },
};

import apiClient from './apiClient';
import {
  Sponsor,
  SponsorDetail,
  CreateSponsorDto,
  UpdateSponsorDto,
  ApiResponse,
  Pagination,
} from '../types';

interface SponsorListResult {
  data: Sponsor[];
  pagination: Pagination;
}

export const sponsorService = {
  getSponsors: async (page = 1, pageSize = 10): Promise<SponsorListResult> => {
    try {
      const response = await apiClient.get('/sponsors', {
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
      throw error.response?.data || { success: false, message: 'Failed to load sponsors' };
    }
  },

  getSponsorById: async (id: number): Promise<SponsorDetail | null> => {
    try {
      const response = await apiClient.get(`/sponsors/${id}`);
      return response.data.data || null;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to load sponsor details' };
    }
  },

  createSponsor: async (payload: CreateSponsorDto): Promise<ApiResponse<Sponsor>> => {
    try {
      const response = await apiClient.post('/sponsors', payload);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to create sponsor' };
    }
  },

  updateSponsor: async (id: number, payload: UpdateSponsorDto): Promise<ApiResponse<Sponsor>> => {
    try {
      const response = await apiClient.put(`/sponsors/${id}`, payload);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to update sponsor' };
    }
  },

  deleteSponsor: async (id: number): Promise<ApiResponse<void>> => {
    try {
      const response = await apiClient.delete(`/sponsors/${id}`);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to delete sponsor' };
    }
  },
};

import apiClient from './apiClient';
import { Club } from '../types';

interface ClubListResponse {
  success: boolean;
  data: {
    data: Club[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  };
  message: string;
}

export const clubService = {
  async getClubs(page = 1, pageSize = 100): Promise<Club[]> {
    try {
      const response = await apiClient.get<ClubListResponse>('/clubs', {
        params: { page, pageSize },
      });

      if (response.data.success) {
        return response.data.data.data;
      }

      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to fetch clubs');
    }
  },
};

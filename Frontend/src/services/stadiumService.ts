import apiClient from './apiClient';
import { Stadium } from '../types';

interface StadiumListResponse {
  success: boolean;
  data: Stadium[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
  message: string;
}

export const stadiumService = {
  async getStadiums(page = 1, pageSize = 100): Promise<Stadium[]> {
    try {
      const response = await apiClient.get<StadiumListResponse>('/stadiums', {
        params: { page, pageSize },
      });

      if (response.data.success) {
        return response.data.data;
      }

      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to fetch stadiums');
    }
  },
};

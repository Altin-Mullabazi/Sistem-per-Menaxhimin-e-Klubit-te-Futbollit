import apiClient from './apiClient';
import { Club, ClubDetail, CreateClubDto, UpdateClubDto, ApiResponse, ClubListResponse } from '../types';

export const clubService = {
  getClubs: async (page: number = 1, pageSize: number = 10, search?: string, city?: string): Promise<ClubListResponse> => {
    try {
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', pageSize.toString());
      if (search) params.append('search', search);
      if (city) params.append('city', city);

      const response = await apiClient.get(`/clubs?${params.toString()}`);
      return response.data.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch clubs' };
    }
  },

  getClubById: async (id: number): Promise<ClubDetail> => {
    try {
      const response = await apiClient.get(`/clubs/${id}`);
      return response.data.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch club' };
    }
  },

  createClub: async (club: CreateClubDto): Promise<ApiResponse<Club>> => {
    try {
      const response = await apiClient.post('/clubs', club);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to create club' };
    }
  },

  updateClub: async (id: number, club: UpdateClubDto): Promise<ApiResponse<Club>> => {
    try {
      const response = await apiClient.put(`/clubs/${id}`, club);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to update club' };
    }
  },

  deleteClub: async (id: number): Promise<ApiResponse<void>> => {
    try {
      const response = await apiClient.delete(`/clubs/${id}`);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to delete club' };
    }
  },

  getAllClubs: async (): Promise<Club[]> => {
    try {
      // Use page=1 and pageSize=100 (backend allows up to 100)
      const response = await apiClient.get('/clubs?page=1&pageSize=100');
      return response.data.data?.data || [];
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch clubs' };
    }
  },
};

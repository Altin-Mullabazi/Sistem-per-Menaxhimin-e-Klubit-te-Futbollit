import apiClient from './apiClient';
import { Player, CreatePlayerDto, UpdatePlayerDto, ApiResponse, PlayerListResponse } from '../types';

export const playerService = {
  getAllPlayers: async (): Promise<Player[]> => {
    try {
      const response = await apiClient.get('/players?pageSize=1000');
      return response.data.data?.data || response.data.data || [];
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch players' };
    }
  },

  getPlayers: async (page: number = 1, pageSize: number = 10, search?: string, position?: string, clubId?: number): Promise<PlayerListResponse> => {
    try {
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', pageSize.toString());
      if (search) params.append('search', search);
      if (position) params.append('position', position);
      if (clubId) params.append('clubId', clubId.toString());

      const response = await apiClient.get(`/players?${params.toString()}`);
      return response.data.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch players' };
    }
  },

  getPlayerById: async (id: number): Promise<Player | null> => {
    try {
      const response = await apiClient.get(`/players/${id}`);
      return response.data.data || null;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch player' };
    }
  },

  createPlayer: async (player: CreatePlayerDto): Promise<ApiResponse<Player>> => {
    try {
      const response = await apiClient.post('/players', player);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to create player' };
    }
  },

  updatePlayer: async (id: number, player: UpdatePlayerDto): Promise<ApiResponse<Player>> => {
    try {
      const response = await apiClient.put(`/players/${id}`, player);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to update player' };
    }
  },

  deletePlayer: async (id: number): Promise<ApiResponse<void>> => {
    try {
      const response = await apiClient.delete(`/players/${id}`);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to delete player' };
    }
  },
};

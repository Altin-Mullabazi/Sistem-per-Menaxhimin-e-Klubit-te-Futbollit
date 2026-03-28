import apiClient from './apiClient';
import { Player, CreatePlayerDto, UpdatePlayerDto, ApiResponse } from '../types';

export const playerService = {
  getAllPlayers: async (): Promise<Player[]> => {
    try {
      const response = await apiClient.get('/players');
      return response.data.data || [];
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

import apiClient from './apiClient';
import { Injury, CreateInjuryDto, UpdateInjuryDto } from '../types';

export const injuryService = {
  getInjuries: async (page = 1, pageSize = 10): Promise<{ items: Injury[]; pagination: any }> => {
    try {
      const response = await apiClient.get('/injuries', { params: { page, pageSize } });
      return response.data.data || { items: [], pagination: { page, pageSize, totalCount: 0, totalPages: 0 } };
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch injuries' };
    }
  },

  getActive: async (): Promise<Injury[]> => {
    try {
      const response = await apiClient.get('/injuries/active');
      return response.data.data || [];
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch active injuries' };
    }
  },

  createInjury: async (payload: CreateInjuryDto) => {
    try {
      const response = await apiClient.post('/injuries', payload);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to create injury' };
    }
  },

  updateInjury: async (id: number, payload: UpdateInjuryDto) => {
    try {
      const response = await apiClient.put(`/injuries/${id}`, payload);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to update injury' };
    }
  },

  deleteInjury: async (id: number) => {
    try {
      const response = await apiClient.delete(`/injuries/${id}`);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to delete injury' };
    }
  },
};

export default injuryService;
